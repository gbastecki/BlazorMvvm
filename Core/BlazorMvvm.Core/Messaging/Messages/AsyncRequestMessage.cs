using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BlazorMvvm;

/// <summary>
/// A base message class for asynchronous request/response pattern.
/// </summary>
/// <typeparam name="T">The type of the response value.</typeparam>
public class AsyncRequestMessage<T>
{
    private Task<T>? _responseTask;

    /// <summary>
    /// Gets the response task.
    /// </summary>
    public Task<T> Response
    {
        get
        {
            if (_responseTask == null)
            {
                throw new InvalidOperationException("No response has been received for this message.");
            }
            return _responseTask;
        }
    }

    /// <summary>
    /// Gets a value indicating whether a response has been received.
    /// </summary>
    public bool HasReceivedResponse => _responseTask != null;

    /// <summary>
    /// Replies to the message with the specified response.
    /// </summary>
    /// <param name="response">The response value.</param>
    public void Reply(T response)
    {
        Reply(Task.FromResult(response));
    }

    /// <summary>
    /// Replies to the message with the specified response task.
    /// </summary>
    /// <param name="response">The response task.</param>
    public void Reply(Task<T> response)
    {
        if (_responseTask != null)
        {
            throw new InvalidOperationException("A response has already been received for this message.");
        }
        _responseTask = response ?? throw new ArgumentNullException(nameof(response));
    }

    /// <summary>
    /// Gets an awaiter for the response.
    /// </summary>
    public TaskAwaiter<T> GetAwaiter()
    {
        return Response.GetAwaiter();
    }
}
