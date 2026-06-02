namespace Jukebox.DataAccess.Contracts.DataContracts.Artist;

public class ListArtistsRequest
{
    public string? NameSearch { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
