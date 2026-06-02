using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Song;

public class ListSongsResult
{
    public bool Success { get; set; }
    public List<SongSummary> Songs { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? ErrorMessage { get; set; }
}
