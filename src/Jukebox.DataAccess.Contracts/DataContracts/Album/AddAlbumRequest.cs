using System;
using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Album;

public class AddAlbumRequest
{
    public string Title { get; set; } = string.Empty;
    public List<int> ArtistIds { get; set; } = [];
    public DateTime? ReleaseDate { get; set; }
    public bool IsCompilation { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? UserId { get; set; }
}