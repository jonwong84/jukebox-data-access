using Jukebox.DataAccess.Contracts.DataContracts.Album;
using System;
using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Artist;

public class ArtistDetails
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Bio { get; set; } = string.Empty;
    public List<AlbumSummary> Albums { get; set; } = [];
}