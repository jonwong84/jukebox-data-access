namespace JukeboxDataAccess.DataModels;

/// <summary>
/// Represents the raw data returned from the SQL database for a song row.
/// </summary>
public class SongDataModel
{
    /// <summary>Gets or sets the unique identifier of the song.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the title of the song.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the artist who performs the song.</summary>
    public string Artist { get; set; } = string.Empty;

    /// <summary>Gets or sets the album the song belongs to.</summary>
    public string Album { get; set; } = string.Empty;

    /// <summary>Gets or sets the genre of the song.</summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>Gets or sets the duration of the song in seconds.</summary>
    public int DurationSeconds { get; set; }

    /// <summary>Gets or sets the year the song was released.</summary>
    public int ReleaseYear { get; set; }
}
