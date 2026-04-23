namespace JukeboxDataAccess.Requests;

/// <summary>
/// Represents a request to retrieve a single song by its unique identifier.
/// </summary>
public class GetSongRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the song to retrieve.
    /// </summary>
    public int Id { get; set; }
}
