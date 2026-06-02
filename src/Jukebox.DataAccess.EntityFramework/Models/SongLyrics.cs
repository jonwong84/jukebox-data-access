namespace Jukebox.DataAccess.EntityFramework.Models;

public class SongLyrics
{
    public int Id { get; set; }
    public int SongId { get; set; }
    public Song Song { get; set; } = null!;
    public string Lyrics { get; set; } = string.Empty;
}
