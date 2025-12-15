namespace Tracker.Core.Services.Contracts;

/// <summary>
/// Provides functionality to generate and compare ETags based entity timestamp, and an optional suffix.
/// </summary>
public interface IETagProvider
{
    /// <summary>
    /// Compares a provided ETag against expected values to determine if they match.
    /// </summary>
    /// <param name="etag">The ETag to compare.</param>
    /// <param name="lastTimestamp">The last modified timestamp of the entity.</param>
    /// <param name="suffix">Optional suffix used in ETag generation (e.g., content hash).</param>
    /// <returns>
    /// <c>true</c> if the provided ETag matches the expected format and values;
    /// otherwise, <c>false</c>.
    /// </returns>
    bool Compare(string etag, ulong lastTimestamp, string suffix);

    /// <summary>
    /// Generates an ETag based on the entity's last modified timestamp and an optional suffix.
    /// </summary>
    /// <param name="lastTimestamp">The last modified timestamp of the entity.</param>
    /// <param name="suffix">Optional suffix to include in the ETag (e.g., content hash).</param>
    /// <returns>
    /// ETag as string
    /// </returns>
    string Generate(ulong lastTimestamp, string suffix);
}
