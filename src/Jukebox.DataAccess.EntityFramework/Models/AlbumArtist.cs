namespace Jukebox.DataAccess.EntityFramework.Models;

public class AlbumArtist
{
    public int AlbumId { get; set; }
    public Album Album { get; set; } = null!;
    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;
}
