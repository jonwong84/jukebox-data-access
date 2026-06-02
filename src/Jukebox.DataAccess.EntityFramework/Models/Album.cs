using System;
using System.Collections.Generic;

namespace Jukebox.DataAccess.EntityFramework.Models;

public class Album
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public ICollection<Song> Songs { get; set; } = new List<Song>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public AlbumDescription? Description { get; set; }
    public ICollection<AlbumArtist> AlbumArtists { get; set; } = new List<AlbumArtist>();
    public bool IsCompilation { get; set; } = false;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
