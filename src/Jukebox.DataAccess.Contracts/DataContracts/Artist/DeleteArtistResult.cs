namespace Jukebox.DataAccess.Contracts.DataContracts.Artist;

public class DeleteArtistResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}