namespace Jukebox.DataAccess.Contracts.DataContracts.Genre;

public class GenreSummary
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? ParentGenreId { get; set; }
}