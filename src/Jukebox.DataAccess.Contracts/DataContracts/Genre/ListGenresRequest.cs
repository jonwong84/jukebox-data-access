namespace Jukebox.DataAccess.Contracts.DataContracts.Genre;

public class ListGenresRequest
{
    public string? NameSearch { get; set; }
    public int? ParentGenreId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}