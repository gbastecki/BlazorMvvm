using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BlazorMvvm;

/// <summary>
/// A messenger implementation that uses strong references to track recipients.
/// Recipients must be explicitly unregistered to avoid memory leaks.
/// </summary>
public sealed class BlazorStrongMessenger : IBlazorMessenger
{
    private static BlazorStrongMessenger? _default;

    /// <summary>
    /// Gets the default singleton instance of <see cref="BlazorStrongMessenger"/>.
    /// </summary>
    public static BlazorStrongMessenger Default => _default ??= new BlazorStrongMessenger();

    private readonly object _lock = new();
    private readonly Dictionary<object, RecipientData> _recipientData = new();

    private sealed class RecipientData
    {
        public Dictionary<Type, Dictionary<object, object>> Handlers { get; } = new();
    }

    private sealed class Channel : IEquatable<Channel>
    {
        public static readonly Channel Default = new();
        public bool Equals(Channel? other) => ReferenceEquals(this, other);
        public override bool Equals(object? obj) => obj is Channel other && Equals(other);
        public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
    }

    /// <inheritdoc/>
    public void Register<TMessage>(object recipient, Action<object, TMessage> handler) where TMessage : class
    {
        Register(recipient, Channel.Default, handler);
    }

    /// <inheritdoc/>
    public void Register<TMessage, TToken>(object recipient, TToken token, Action<object, TMessage> handler) where TMessage : class where TToken : IEquatable<TToken>
    {
        ArgumentNullException.ThrowIfNull(recipient);
        ArgumentNullException.ThrowIfNull(handler);

        lock (_lock)
        {
            if (!_recipientData.TryGetValue(recipient, out RecipientData? data))
            {
                data = new RecipientData();
                _recipientData[recipient] = data;
            }

            Type messageType = typeof(TMessage);

            if (!data.Handlers.TryGetValue(messageType, out Dictionary<object, object>? tokenHandlers))
            {
                tokenHandlers = new Dictionary<object, object>();
                data.Handlers[messageType] = tokenHandlers;
            }

            tokenHandlers[token!] = handler;
        }
    }

    /// <inheritdoc/>
    public void Unregister<TMessage>(object recipient) where TMessage : class
    {
        Unregister<TMessage, Channel>(recipient, Channel.Default);
    }

    /// <inheritdoc/>
    public void Unregister<TMessage, TToken>(object recipient, TToken token) where TMessage : class where TToken : IEquatable<TToken>
    {
        ArgumentNullException.ThrowIfNull(recipient);

        lock (_lock)
        {
            if (_recipientData.TryGetValue(recipient, out RecipientData? data))
            {
                Type messageType = typeof(TMessage);
                if (data.Handlers.TryGetValue(messageType, out Dictionary<object, object>? tokenHandlers))
                {
                    tokenHandlers.Remove(token!);
                    if (tokenHandlers.Count == 0)
                    {
                        data.Handlers.Remove(messageType);
                    }
                }

                if (data.Handlers.Count == 0)
                {
                    _recipientData.Remove(recipient);
                }
            }
        }
    }

    /// <inheritdoc/>
    public void UnregisterAll(object recipient)
    {
        ArgumentNullException.ThrowIfNull(recipient);

        lock (_lock)
        {
            _recipientData.Remove(recipient);
        }
    }

    /// <inheritdoc/>
    public TMessage Send<TMessage>(TMessage message) where TMessage : class
    {
        return Send(message, Channel.Default);
    }

    /// <inheritdoc/>
    public TMessage Send<TMessage, TToken>(TMessage message, TToken token) where TMessage : class where TToken : IEquatable<TToken>
    {
        ArgumentNullException.ThrowIfNull(message);

        List<(object Recipient, Action<object, TMessage> Handler)> recipientHandlers = new();

        lock (_lock)
        {
            Type messageType = typeof(TMessage);

            foreach (KeyValuePair<object, RecipientData> kvp in _recipientData)
            {
                object recipient = kvp.Key;
                RecipientData data = kvp.Value;

                if (data.Handlers.TryGetValue(messageType, out Dictionary<object, object>? tokenHandlers))
                {
                    if (tokenHandlers.TryGetValue(token!, out object? handlerObj))
                    {
                        Action<object, TMessage> handler = (Action<object, TMessage>)handlerObj;
                        recipientHandlers.Add((recipient, handler));
                    }
                }
            }
        }

        foreach ((object recipient, Action<object, TMessage> handler) in recipientHandlers)
        {
            handler(recipient, message);
        }

        return message;
    }

    /// <inheritdoc/>
    public bool IsRegistered<TMessage>(object recipient) where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(recipient);

        lock (_lock)
        {
            if (_recipientData.TryGetValue(recipient, out RecipientData? data))
            {
                return data.Handlers.ContainsKey(typeof(TMessage));
            }
        }

        return false;
    }

    /// <summary>
    /// Resets the default instance. Used for testing.
    /// </summary>
    internal static void ResetDefault()
    {
        _default = null;
    }
}
