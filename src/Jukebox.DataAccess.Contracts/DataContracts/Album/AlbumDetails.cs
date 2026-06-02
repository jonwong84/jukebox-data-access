using Jukebox.DataAccess.Contracts.DataContracts.Artist;
using System;
using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Album;

public class AlbumDetails
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public List<ArtistSummary> Artists { get; set; } = [];
    public DateTime? ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsCompilation { get; set; }
    public string Description { get; set; } = string.Empty;
}