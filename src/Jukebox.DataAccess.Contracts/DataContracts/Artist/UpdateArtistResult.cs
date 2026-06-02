namespace Jukebox.DataAccess.Contracts.DataContracts.Artist;

public class UpdateArtistResult
{
    public bool Success { get; set; }
    public ArtistDetails? ArtistDetails { get; set; }
    public string? ErrorMessage { get; set; }
}