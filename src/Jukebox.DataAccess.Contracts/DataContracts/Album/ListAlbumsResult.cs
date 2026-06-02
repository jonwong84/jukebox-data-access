using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Album;

public class ListAlbumsResult
{
    public bool Success { get; set; }
    public List<AlbumSummary> Albums { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? ErrorMessage { get; set; }
}
