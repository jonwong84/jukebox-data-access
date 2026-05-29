namespace Jukebox.DataAccess.Contracts.DataContracts.Song;

public class ListSongsRequest
{
    public int? ArtistId { get; set; }
    public int? AlbumId { get; set; }
    public int? GenreId { get; set; }
    public int? MinBpm { get; set; }
    public int? MaxBpm { get; set; }
    public string? TitleSearch { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? UserId { get; set; }
}
