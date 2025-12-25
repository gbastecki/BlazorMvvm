using System;

namespace BlazorMvvm;

/// <summary>
/// An interface for a type providing the ability to exchange messages between different objects.
/// </summary>
public interface IBlazorMessenger
{
    /// <summary>
    /// Registers a recipient for a given type of message.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to receive.</typeparam>
    /// <param name="recipient">The recipient that will receive messages.</param>
    /// <param name="handler">The action to invoke when a message is received.</param>
    void Register<TMessage>(object recipient, Action<object, TMessage> handler) where TMessage : class;

    /// <summary>
    /// Registers a recipient for a given type of message on a specific channel.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to receive.</typeparam>
    /// <typeparam name="TToken">The type of token that identifies the channel.</typeparam>
    /// <param name="recipient">The recipient that will receive messages.</param>
    /// <param name="token">The token identifying the channel.</param>
    /// <param name="handler">The action to invoke when a message is received.</param>
    void Register<TMessage, TToken>(object recipient, TToken token, Action<object, TMessage> handler) where TMessage : class where TToken : IEquatable<TToken>;

    /// <summary>
    /// Unregisters a recipient from receiving messages of a given type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to unregister from.</typeparam>
    /// <param name="recipient">The recipient to unregister.</param>
    void Unregister<TMessage>(object recipient) where TMessage : class;

    /// <summary>
    /// Unregisters a recipient from receiving messages of a given type on a specific channel.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to unregister from.</typeparam>
    /// <typeparam name="TToken">The type of token that identifies the channel.</typeparam>
    /// <param name="recipient">The recipient to unregister.</param>
    /// <param name="token">The token identifying the channel.</param>
    void Unregister<TMessage, TToken>(object recipient, TToken token) where TMessage : class where TToken : IEquatable<TToken>;

    /// <summary>
    /// Unregisters a recipient from all messages.
    /// </summary>
    /// <param name="recipient">The recipient to unregister.</param>
    void UnregisterAll(object recipient);

    /// <summary>
    /// Sends a message of the specified type to all registered recipients.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to send.</typeparam>
    /// <param name="message">The message to send.</param>
    /// <returns>The message that was sent.</returns>
    TMessage Send<TMessage>(TMessage message) where TMessage : class;

    /// <summary>
    /// Sends a message of the specified type to all registered recipients on a specific channel.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to send.</typeparam>
    /// <typeparam name="TToken">The type of token that identifies the channel.</typeparam>
    /// <param name="message">The message to send.</param>
    /// <param name="token">The token identifying the channel.</param>
    /// <returns>The message that was sent.</returns>
    TMessage Send<TMessage, TToken>(TMessage message, TToken token) where TMessage : class where TToken : IEquatable<TToken>;

    /// <summary>
    /// Checks whether a given recipient is registered for a specific message type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to check.</typeparam>
    /// <param name="recipient">The recipient to check.</param>
    /// <returns><c>true</c> if the recipient is registered; otherwise, <c>false</c>.</returns>
    bool IsRegistered<TMessage>(object recipient) where TMessage : class;
}
