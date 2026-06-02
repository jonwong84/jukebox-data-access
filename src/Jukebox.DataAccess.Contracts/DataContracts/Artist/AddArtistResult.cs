namespace Jukebox.DataAccess.Contracts.DataContracts.Artist;

public class AddArtistResult
{
    public bool Success { get; set; }
    public int? ArtistId { get; set; }
    public string? ErrorMessage { get; set; }
}