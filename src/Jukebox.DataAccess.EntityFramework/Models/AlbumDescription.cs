namespace Jukebox.DataAccess.EntityFramework.Models;

public class AlbumDescription
{
    public int Id { get; set; }
    public int AlbumId { get; set; }
    public Album Album { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
}
