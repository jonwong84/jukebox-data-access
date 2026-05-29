namespace Jukebox.DataAccess.Contracts.DataContracts.Album;

public class ListAlbumsRequest
{
    public int? ArtistId { get; set; }
    public int? GenreId { get; set; }
    public string? TitleSearch { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? UserId { get; set; }
}
