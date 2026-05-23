using System.Collections.Generic;

namespace Jukebox.DataAccess.EntityFramework.Models;

public class Genre
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<SongGenre> SongGenres { get; set; } = new List<SongGenre>();
    public int? ParentGenreId { get; set; }
    public Genre? ParentGenre { get; set; }
    public ICollection<Genre> SubGenres { get; set; } = new List<Genre>();
}
