namespace Jukebox.DataAccess.Contracts.DataContracts.Album;

public class AddAlbumResult
{
    public bool Success { get; set; }
    public int? AlbumId { get; set; }
    public string? ErrorMessage { get; set; }
}