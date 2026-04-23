namespace JukeboxDataAccess.Requests;

/// <summary>
/// Represents a request to search for songs using one or more optional criteria.
/// </summary>
public class SearchSongsRequest
{
    /// <summary>
    /// Gets or sets an optional title filter (partial match).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets an optional artist filter (partial match).
    /// </summary>
    public string? Artist { get; set; }

    /// <summary>
    /// Gets or sets an optional genre filter (exact match).
    /// </summary>
    public string? Genre { get; set; }

    /// <summary>
    /// Gets or sets an optional album filter (partial match).
    /// </summary>
    public string? Album { get; set; }

    /// <summary>
    /// Gets or sets an optional release year filter.
    /// </summary>
    public int? ReleaseYear { get; set; }
}
