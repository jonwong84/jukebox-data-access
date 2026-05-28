namespace Jukebox.DataAccess.Contracts.DataContracts.Album;

public class DeleteAlbumResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}