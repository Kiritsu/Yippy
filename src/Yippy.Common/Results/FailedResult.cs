namespace Yippy.Common.Results;

/// <summary>
/// Represents a service result that has failed.
/// </summary>
/// <param name="key">The key of the result.</param>
/// <param name="message">The message associated with the result.</param>
public class FailedResult(string? key = null, string? message = null) : YippyResult(false, key, message);