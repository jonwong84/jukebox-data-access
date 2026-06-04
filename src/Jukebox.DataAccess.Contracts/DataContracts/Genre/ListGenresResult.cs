using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Genre;

public class ListGenresResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public ICollection<GenreSummary> Genres { get; set; } = new List<GenreSummary>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}