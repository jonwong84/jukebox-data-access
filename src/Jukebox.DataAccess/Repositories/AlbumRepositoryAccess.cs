using Jukebox.DataAccess.Contracts.DataContracts.Album;
using Jukebox.DataAccess.Contracts.DataContracts.Artist;
using Jukebox.DataAccess.EntityFramework.Models;
using Jukebox.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Jukebox.DataAccess.Repositories;

public class AlbumRepositoryAccess(EntityFramework.JukeboxDbContext context, ILogger<AlbumRepositoryAccess> logger) : IAlbumRepositoryAccess
{
    private readonly EntityFramework.JukeboxDbContext _context = context;
    private readonly ILogger _logger = logger;

    public async Task<GetAlbumResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var album = await _context.Albums
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Artist)
                .Include(a => a.Description)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

            if (album is null)
            {
                return new GetAlbumResult
                {
                    Success = false,
                    ErrorMessage = $"Album with ID {id} was not found.",
                };
            }

            return new GetAlbumResult
            {
                Success = true,
                AlbumDetails = MapToAlbumDetails(album),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve album with ID {AlbumId}", id);
            return new GetAlbumResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    public async Task<AddAlbumResult> AddAsync(AddAlbumRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.ArtistIds.Count != 0)
            {
                var existingArtistIds = await _context.Artists
                    .Where(a => request.ArtistIds.Contains(a.Id))
                    .Select(a => a.Id)
                    .ToListAsync(cancellationToken);

                var invalidArtistIds = request.ArtistIds.Except(existingArtistIds).ToList();

                if (invalidArtistIds.Count != 0)
                {
                    return new AddAlbumResult
                    {
                        Success = false,
                        ErrorMessage = $"The following artist IDs do not exist: {string.Join(", ", invalidArtistIds)}",
                    };
                }
            }

            var album = new Album
            {
                Title = request.Title,
                ReleaseDate = request.ReleaseDate,
                IsCompilation = request.IsCompilation,
                CreatedBy = request.UserId,
                AlbumArtists = request.ArtistIds
                    .Select(id => new AlbumArtist { ArtistId = id })
                    .ToList(),
            };

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                album.Description = new AlbumDescription
                {
                    Description = request.Description,
                };
            }

            await _context.Albums.AddAsync(album, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new AddAlbumResult
            {
                Success = true,
                AlbumId = album.Id,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add album with title {Title}", request.Title);
            return new AddAlbumResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    public async Task<UpdateAlbumResult> UpdateAsync(UpdateAlbumRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var album = await _context.Albums
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Artist)
                .Include(a => a.Description)
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (album is null)
            {
                return new UpdateAlbumResult
                {
                    Success = false,
                    ErrorMessage = $"Album with ID {request.Id} was not found.",
                };
            }

            if (request.ArtistIds.Count != 0)
            {
                var existingArtistIds = await _context.Artists
                    .Where(a => request.ArtistIds.Contains(a.Id))
                    .Select(a => a.Id)
                    .ToListAsync(cancellationToken);

                var invalidArtistIds = request.ArtistIds.Except(existingArtistIds).ToList();

                if (invalidArtistIds.Count != 0)
                {
                    return new UpdateAlbumResult
                    {
                        Success = false,
                        ErrorMessage = $"The following artist IDs do not exist: {string.Join(", ", invalidArtistIds)}",
                    };
                }
            }

            album.Title = request.Title;
            album.ReleaseDate = request.ReleaseDate;
            album.IsCompilation = request.IsCompilation;
            album.UpdatedBy = request.UserId;

            album.AlbumArtists = request.ArtistIds
                .Select(id => new AlbumArtist { AlbumId = album.Id, ArtistId = id })
                .ToList();

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                if (album.Description is null)
                {
                    album.Description = new AlbumDescription { Description = request.Description };
                }
                else
                {
                    album.Description.Description = request.Description;
                }
            }
            else if (album.Description is not null)
            {
                _context.Remove(album.Description);
            }

            await _context.SaveChangesAsync(cancellationToken);

            await _context.Entry(album)
                .Collection(a => a.AlbumArtists)
                .Query()
                .Include(aa => aa.Artist)
                .LoadAsync(cancellationToken);

            return new UpdateAlbumResult
            {
                Success = true,
                AlbumDetails = MapToAlbumDetails(album),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update album with ID {AlbumId}", request.Id);
            return new UpdateAlbumResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    public async Task<DeleteAlbumResult> DeleteAsync(int albumId, CancellationToken cancellationToken = default)
    {
        try
        {
            var album = await _context.Albums
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(a => a.Id == albumId, cancellationToken);

            if (album is null)
            {
                return new DeleteAlbumResult
                {
                    Success = false,
                    ErrorMessage = $"Album with ID {albumId} was not found.",
                };
            }

            if (album.Songs.Count != 0)
            {
                return new DeleteAlbumResult
                {
                    Success = false,
                    ErrorMessage = $"Album with ID {albumId} cannot be deleted because it has {album.Songs.Count} song(s) associated with it.",
                };
            }

            _context.Albums.Remove(album);
            await _context.SaveChangesAsync(cancellationToken);

            return new DeleteAlbumResult
            {
                Success = true,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete album with ID {AlbumId}", albumId);
            return new DeleteAlbumResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    private static AlbumDetails MapToAlbumDetails(Album album) => new()
    {
        Id = album.Id,
        Title = album.Title,
        Artists = album.AlbumArtists
            .Select(aa => new ArtistSummary
            {
                Id = aa.Artist.Id,
                Name = aa.Artist.Name,
            })
            .ToList(),
        ReleaseDate = album.ReleaseDate,
        CreatedAt = album.CreatedAt,
        IsCompilation = album.IsCompilation,
        Description = album.Description?.Description ?? string.Empty,
    };

    public async Task<ListAlbumsResult> ListAsync(ListAlbumsRequest request, CancellationToken cancellationToken = default)
    {
        var pageSize = Math.Min(request.PageSize, 100);
        var pageNumber = Math.Max(request.PageNumber, 1);

        var query = _context.Albums
            .Include(a => a.AlbumArtists)
                .ThenInclude(aa => aa.Artist)
            .AsQueryable();

        if (request.ArtistId.HasValue)
            query = query.Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == request.ArtistId.Value));

        if (request.GenreId.HasValue)
            query = query.Where(a => a.Songs.Any(s => s.SongGenres.Any(sg => sg.GenreId == request.GenreId.Value)));

        if (!string.IsNullOrWhiteSpace(request.TitleSearch))
            query = query.Where(a => a.Title.Contains(request.TitleSearch));

        var totalCount = await query.CountAsync(cancellationToken);

        var albums = await query
            .OrderBy(a => a.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ListAlbumsResult
        {
            Success = true,
            Albums = albums.Select(a => new AlbumSummary
            {
                Id = a.Id,
                Title = a.Title,
                Artists = a.AlbumArtists.Select(aa => new ArtistSummary
                {
                    Id = aa.Artist.Id,
                    Name = aa.Artist.Name
                }).ToList()
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}