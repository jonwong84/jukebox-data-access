using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Artist;

public class ListArtistsResult
{
    public bool Success { get; set; }
    public List<ArtistSummary> Artists { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? ErrorMessage { get; set; }
}
