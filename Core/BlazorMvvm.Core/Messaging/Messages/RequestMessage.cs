using System;

namespace BlazorMvvm;

/// <summary>
/// A base message class for request/response pattern.
/// </summary>
/// <typeparam name="T">The type of the response value.</typeparam>
public class RequestMessage<T>
{
    private T? _response;
    private bool _hasResponse;

    /// <summary>
    /// Gets the response value.
    /// </summary>
    public T Response
    {
        get
        {
            if (!_hasResponse)
            {
                throw new InvalidOperationException("No response has been received for this message.");
            }
            return _response!;
        }
    }

    /// <summary>
    /// Gets a value indicating whether a response has been received.
    /// </summary>
    public bool HasReceivedResponse => _hasResponse;

    /// <summary>
    /// Replies to the message with the specified response.
    /// </summary>
    /// <param name="response">The response value.</param>
    public void Reply(T response)
    {
        if (_hasResponse)
        {
            throw new InvalidOperationException("A response has already been received for this message.");
        }
        _response = response;
        _hasResponse = true;
    }

    /// <summary>
    /// Implicit conversion to the response type.
    /// </summary>
    public static implicit operator T(RequestMessage<T> message)
    {
        return message.Response;
    }
}
