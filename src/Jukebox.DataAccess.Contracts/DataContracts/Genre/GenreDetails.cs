using System;
using System.Collections.Generic;

namespace Jukebox.DataAccess.Contracts.DataContracts.Genre;

public class GenreDetails
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? ParentGenreId { get; set; }
    public GenreSummary? ParentGenre { get; set; }
    public ICollection<GenreSummary> SubGenres { get; set; } = new List<GenreSummary>();
}