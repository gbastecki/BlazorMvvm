using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BlazorMvvm;

/// <summary>
/// A messenger implementation that uses weak references to track recipients.
/// This is the default messenger that allows recipients to be garbage collected when they are no longer referenced elsewhere.
/// </summary>
public sealed class BlazorMessenger : IBlazorMessenger
{
    private static BlazorMessenger? _default;

    /// <summary>
    /// Gets the default singleton instance of <see cref="BlazorMessenger"/>.
    /// </summary>
    public static BlazorMessenger Default => _default ??= new BlazorMessenger();

    private readonly object _lock = new();
    private readonly ConditionalWeakTable<object, RecipientData> _recipientData = new();

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
            RecipientData data = _recipientData.GetOrCreateValue(recipient);
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
            }
        }
    }

    /// <inheritdoc/>
    public void UnregisterAll(object recipient)
    {
        ArgumentNullException.ThrowIfNull(recipient);

        lock (_lock)
        {
            if (_recipientData.TryGetValue(recipient, out RecipientData? data))
            {
                data.Handlers.Clear();
            }
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

        List<Action<object, TMessage>> handlersToInvoke = new();
        List<(object Recipient, Action<object, TMessage> Handler)> recipientHandlers = new();

        lock (_lock)
        {
            Type messageType = typeof(TMessage);
            foreach (KeyValuePair<object, RecipientData> kvp in GetAllRecipients())
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

    private IEnumerable<KeyValuePair<object, RecipientData>> GetAllRecipients()
    {
        foreach (KeyValuePair<object, RecipientData> kvp in _recipientData)
        {
            yield return kvp;
        }
    }

    /// <summary>
    /// Resets the default instance. Used for testing.
    /// </summary>
    internal static void ResetDefault()
    {
        _default = null;
    }
}
