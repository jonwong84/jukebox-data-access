namespace Jukebox.DataAccess.Contracts.DataContracts.Album;

public class UpdateAlbumResult
{
    public bool Success { get; set; }
    public AlbumDetails? AlbumDetails { get; set; }
    public string? ErrorMessage { get; set; }
}