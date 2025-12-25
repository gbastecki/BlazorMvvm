namespace BlazorMvvm;

/// <summary>
/// An interface for a recipient that can receive messages of the specified type.
/// </summary>
/// <typeparam name="TMessage">The type of message to receive.</typeparam>
public interface IBlazorRecipient<in TMessage> where TMessage : class
{
    /// <summary>
    /// Receives a given message.
    /// </summary>
    /// <param name="message">The message being received.</param>
    void Receive(TMessage message);
}
