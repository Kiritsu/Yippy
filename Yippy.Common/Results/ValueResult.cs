namespace Yippy.Common.Results;

/// <summary>
/// Represents a result that holds a value.
/// </summary>
/// <typeparam name="T">The type of result that is returned.</typeparam>
/// <param name="value">The value of the result.</param>
public class ValueResult<T>(T value) : YippyResult(true)
{
    /// <summary>
    /// Gets the value associated with the result.
    /// </summary>
    public T Value { get; } = value;
}