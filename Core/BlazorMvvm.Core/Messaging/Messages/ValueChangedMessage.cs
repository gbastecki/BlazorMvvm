namespace BlazorMvvm;

/// <summary>
/// A base message class that contains a single value.
/// </summary>
/// <typeparam name="T">The type of value.</typeparam>
public class ValueChangedMessage<T>
{
    /// <summary>
    /// Gets the value that has changed.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueChangedMessage{T}"/> class.
    /// </summary>
    /// <param name="value">The value that has changed.</param>
    public ValueChangedMessage(T value)
    {
        Value = value;
    }
}
