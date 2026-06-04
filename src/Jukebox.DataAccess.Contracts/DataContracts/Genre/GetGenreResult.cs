namespace Jukebox.DataAccess.Contracts.DataContracts.Genre;

public class GetGenreResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public GenreDetails? GenreDetails { get; set; }
}