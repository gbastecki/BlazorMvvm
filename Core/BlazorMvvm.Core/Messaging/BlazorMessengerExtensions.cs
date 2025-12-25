using System;

namespace BlazorMvvm;

/// <summary>
/// Extension methods for registering <see cref="IBlazorRecipient{TMessage}"/> with messengers.
/// </summary>
public static class BlazorMessengerExtensions
{
    /// <summary>
    /// Registers all message handlers for a recipient that implements <see cref="IBlazorRecipient{TMessage}"/>.
    /// </summary>
    /// <typeparam name="TMessage">The type of message.</typeparam>
    /// <param name="messenger">The messenger instance.</param>
    /// <param name="recipient">The recipient to register.</param>
    public static void Register<TMessage>(this IBlazorMessenger messenger, IBlazorRecipient<TMessage> recipient) where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(messenger);
        ArgumentNullException.ThrowIfNull(recipient);

        messenger.Register<TMessage>(recipient, static (r, m) => ((IBlazorRecipient<TMessage>)r).Receive(m));
    }

    /// <summary>
    /// Registers all message handlers for a recipient that implements <see cref="IBlazorRecipient{TMessage}"/> on a specific channel.
    /// </summary>
    /// <typeparam name="TMessage">The type of message.</typeparam>
    /// <typeparam name="TToken">The type of token that identifies the channel.</typeparam>
    /// <param name="messenger">The messenger instance.</param>
    /// <param name="recipient">The recipient to register.</param>
    /// <param name="token">The token identifying the channel.</param>
    public static void Register<TMessage, TToken>(this IBlazorMessenger messenger, IBlazorRecipient<TMessage> recipient, TToken token) where TMessage : class where TToken : IEquatable<TToken>
    {
        ArgumentNullException.ThrowIfNull(messenger);
        ArgumentNullException.ThrowIfNull(recipient);

        messenger.Register<TMessage, TToken>(recipient, token, static (r, m) => ((IBlazorRecipient<TMessage>)r).Receive(m));
    }
}
