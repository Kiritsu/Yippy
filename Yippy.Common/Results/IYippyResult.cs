using System.Diagnostics.CodeAnalysis;

namespace Yippy.Common.Results;

/// <summary>
/// Defines a service result.
/// </summary>
public interface IYippyResult
{
    /// <summary>
    /// The key that identifies the kind of result.
    /// </summary>
    string? Key { get; }

    /// <summary>
    /// The message associated with the result.
    /// </summary>
    string? Message { get; }

    /// <summary>
    /// Whether the result is a success or not.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Key))]
    [MemberNotNullWhen(false, nameof(Message))]
    bool Success { get; }
}

/// <summary>
/// Defines the base of a service result, with the success and a facultative key and message.
/// </summary>
/// <remarks>
/// This class can be serialized in any compatible format.
/// </remarks>
/// <param name="success">Whether the result is successful or not.</param>
/// <param name="key">The key of the result.</param>
/// <param name="message">The message associated with the result.</param>
public class YippyResult(bool success, string? key = null, string? message = null) : IYippyResult
{
    /// <inheritdoc/>
    public string? Key { get; init; } = key;

    /// <inheritdoc/>
    public string? Message { get; init; } = message;

    /// <inheritdoc/>
    [MemberNotNullWhen(false, nameof(Key))]
    [MemberNotNullWhen(false, nameof(Message))]
    public bool Success { get; init; } = success;
}