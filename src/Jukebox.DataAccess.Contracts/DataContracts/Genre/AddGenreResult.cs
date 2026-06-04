namespace Jukebox.DataAccess.Contracts.DataContracts.Genre;

public class AddGenreResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int GenreId { get; set; }
}