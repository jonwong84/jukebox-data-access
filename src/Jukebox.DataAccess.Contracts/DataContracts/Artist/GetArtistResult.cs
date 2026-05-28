namespace Jukebox.DataAccess.Contracts.DataContracts.Artist;

public class GetArtistResult
{
    public bool Success { get; set; }
    public ArtistDetails? ArtistDetails { get; set; }
    public string? ErrorMessage { get; set; }
}