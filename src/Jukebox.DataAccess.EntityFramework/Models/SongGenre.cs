namespace Jukebox.DataAccess.EntityFramework.Models;

public class SongGenre
{
    public int SongId { get; set; }
    public Song Song { get; set; } = null!;
    public int GenreId { get; set; }
    public Genre Genre { get; set; } = null!;
}
