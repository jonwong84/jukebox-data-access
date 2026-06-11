using Jukebox.DataAccess.Contracts.DataContracts.Genre;
using Jukebox.DataAccess.Contracts.Interfaces;
using Jukebox.DataAccess.EntityFramework.Models;
using Jukebox.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Jukebox.DataAccess.Repositories;

public class GenreRepositoryAccess(EntityFramework.JukeboxDbContext context, ILogger<GenreRepositoryAccess> logger) : IGenreRepositoryAccess
{
    private readonly EntityFramework.JukeboxDbContext _context = context;
    private readonly ILogger _logger = logger;

    public async Task<GetGenreResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var genre = await _context.Genres
                .Include(g => g.ParentGenre)
                .Include(g => g.SubGenres)
                .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

            if (genre is null)
            {
                return new GetGenreResult
                {
                    Success = false,
                    ErrorMessage = $"Genre with ID {id} was not found.",
                };
            }

            return new GetGenreResult
            {
                Success = true,
                GenreDetails = MapToGenreDetails(genre),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve genre with ID {GenreId}", id);
            return new GetGenreResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    public async Task<AddGenreResult> AddAsync(AddGenreRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.ParentGenreId.HasValue)
            {
                var parentExists = await _context.Genres
                    .AnyAsync(g => g.Id == request.ParentGenreId.Value, cancellationToken);

                if (!parentExists)
                {
                    return new AddGenreResult
                    {
                        Success = false,
                        ErrorMessage = $"Parent genre with ID {request.ParentGenreId.Value} was not found.",
                    };
                }
            }

            var genre = new Genre
            {
                Name = request.Name,
                Description = request.Description,
                ParentGenreId = request.ParentGenreId,
                CreatedBy = request.UserId,
            };

            await _context.Genres.AddAsync(genre, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new AddGenreResult
            {
                Success = true,
                GenreId = genre.Id,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add genre with name {Name}", request.Name);
            return new AddGenreResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    public async Task<UpdateGenreResult> UpdateAsync(UpdateGenreRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var genre = await _context.Genres
                .Include(g => g.ParentGenre)
                .Include(g => g.SubGenres)
                .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

            if (genre is null)
            {
                return new UpdateGenreResult
                {
                    Success = false,
                    ErrorMessage = $"Genre with ID {request.Id} was not found.",
                };
            }

            if (request.ParentGenreId.HasValue)
            {
                if (request.ParentGenreId.Value == request.Id)
                {
                    return new UpdateGenreResult
                    {
                        Success = false,
                        ErrorMessage = "A genre cannot be its own parent.",
                    };
                }

                var parentExists = await _context.Genres
                    .AnyAsync(g => g.Id == request.ParentGenreId.Value, cancellationToken);

                if (!parentExists)
                {
                    return new UpdateGenreResult
                    {
                        Success = false,
                        ErrorMessage = $"Parent genre with ID {request.ParentGenreId.Value} was not found.",
                    };
                }
            }

            genre.Name = request.Name;
            genre.Description = request.Description;
            genre.ParentGenreId = request.ParentGenreId;
            genre.UpdatedBy = request.UserId;
            genre.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _context.Entry(genre)
                .Reference(g => g.ParentGenre)
                .LoadAsync(cancellationToken);

            return new UpdateGenreResult
            {
                Success = true,
                GenreDetails = MapToGenreDetails(genre),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update genre with ID {GenreId}", request.Id);
            return new UpdateGenreResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    public async Task<DeleteGenreResult> DeleteAsync(int genreId, CancellationToken cancellationToken = default)
    {
        try
        {
            var genre = await _context.Genres
                .Include(g => g.SubGenres)
                .Include(g => g.SongGenres)
                .FirstOrDefaultAsync(g => g.Id == genreId, cancellationToken);

            if (genre is null)
            {
                return new DeleteGenreResult
                {
                    Success = false,
                    ErrorMessage = $"Genre with ID {genreId} was not found.",
                };
            }

            if (genre.SubGenres.Count != 0)
            {
                return new DeleteGenreResult
                {
                    Success = false,
                    ErrorMessage = $"Genre with ID {genreId} cannot be deleted because it has {genre.SubGenres.Count} sub-genre(s) associated with it.",
                };
            }

            if (genre.SongGenres.Count != 0)
            {
                return new DeleteGenreResult
                {
                    Success = false,
                    ErrorMessage = $"Genre with ID {genreId} cannot be deleted because it has {genre.SongGenres.Count} song(s) associated with it.",
                };
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync(cancellationToken);

            return new DeleteGenreResult
            {
                Success = true,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete genre with ID {GenreId}", genreId);
            return new DeleteGenreResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    public async Task<ListGenresResult> ListAsync(ListGenresRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var pageSize = Math.Min(request.PageSize, 100);
            var pageNumber = Math.Max(request.PageNumber, 1);

            var query = _context.Genres.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.NameSearch))
                query = query.Where(g => g.Name.Contains(request.NameSearch));

            if (request.ParentGenreId.HasValue)
                query = query.Where(g => g.ParentGenreId == request.ParentGenreId.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var genres = await query
                .OrderBy(g => g.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new ListGenresResult
            {
                Success = true,
                Genres = genres.Select(g => new GenreSummary
                {
                    Id = g.Id,
                    Name = g.Name,
                    ParentGenreId = g.ParentGenreId,
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list genres");
            return new ListGenresResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    private static GenreDetails MapToGenreDetails(Genre genre) => new()
    {
        Id = genre.Id,
        Name = genre.Name,
        Description = genre.Description ?? string.Empty,
        ParentGenreId = genre.ParentGenreId,
        ParentGenre = genre.ParentGenre is null ? null : new GenreSummary
        {
            Id = genre.ParentGenre.Id,
            Name = genre.ParentGenre.Name,
            ParentGenreId = genre.ParentGenre.ParentGenreId,
        },
        SubGenres = genre.SubGenres.Select(s => new GenreSummary
        {
            Id = s.Id,
            Name = s.Name,
            ParentGenreId = s.ParentGenreId,
        }).ToList(),
    };
}