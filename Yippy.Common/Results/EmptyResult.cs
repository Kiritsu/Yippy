namespace Yippy.Common.Results;

/// <summary>
/// Represents a service result that succeeded with no specific content.
/// </summary>
public class EmptyResult() : YippyResult(true)
{
    /// <summary>
    /// Gets the instance of an empty result.
    /// </summary>
    public static EmptyResult Instance { get; } = new();
}