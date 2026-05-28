using Jukebox.DataAccess.Contracts.DataContracts.Artist;
using Jukebox.DataAccess.Contracts.DataContracts.Album;
using Jukebox.DataAccess.EntityFramework.Models;
using Jukebox.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Jukebox.DataAccess.Artists;

public class ArtistRepositoryAccess(EntityFramework.JukeboxDbContext context, ILogger<ArtistRepositoryAccess> logger) : IArtistRepositoryAccess
{
    private readonly EntityFramework.JukeboxDbContext _context = context;
    private readonly ILogger _logger = logger;

    public async Task<GetArtistResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var artist = await _context.Artists
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Album)
                        .ThenInclude(al => al.AlbumArtists)
                            .ThenInclude(aa => aa.Artist)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

            if (artist is null)
            {
                return new GetArtistResult
                {
                    Success = false,
                    ErrorMessage = $"Artist with ID {id} was not found.",
                };
            }

            return new GetArtistResult
            {
                Success = true,
                ArtistDetails = MapToArtistDetails(artist),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve artist with ID {ArtistId}", id);
            return new GetArtistResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    public async Task<AddArtistResult> AddAsync(AddArtistRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var artist = new Artist
            {
                Name = request.Name,
                Bio = request.Bio,
                CreatedBy = request.UserId,
            };

            await _context.Artists.AddAsync(artist, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new AddArtistResult
            {
                Success = true,
                ArtistId = artist.Id,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add artist with name {Name}", request.Name);
            return new AddArtistResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    public async Task<UpdateArtistResult> UpdateAsync(UpdateArtistRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var artist = await _context.Artists
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Album)
                        .ThenInclude(al => al.AlbumArtists)
                            .ThenInclude(aa => aa.Artist)
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (artist is null)
            {
                return new UpdateArtistResult
                {
                    Success = false,
                    ErrorMessage = $"Artist with ID {request.Id} was not found.",
                };
            }

            artist.Name = request.Name;
            artist.Bio = request.Bio;
            artist.UpdatedBy = request.UserId;

            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateArtistResult
            {
                Success = true,
                ArtistDetails = MapToArtistDetails(artist),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update artist with ID {ArtistId}", request.Id);
            return new UpdateArtistResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    public async Task<DeleteArtistResult> DeleteAsync(int artistId, CancellationToken cancellationToken = default)
    {
        try
        {
            var artist = await _context.Artists
                .Include(a => a.Songs)
                .Include(a => a.AlbumArtists)
                .FirstOrDefaultAsync(a => a.Id == artistId, cancellationToken);

            if (artist is null)
            {
                return new DeleteArtistResult
                {
                    Success = false,
                    ErrorMessage = $"Artist with ID {artistId} was not found.",
                };
            }

            if (artist.Songs.Count != 0)
            {
                return new DeleteArtistResult
                {
                    Success = false,
                    ErrorMessage = $"Artist with ID {artistId} cannot be deleted because they have {artist.Songs.Count} song(s) associated with them.",
                };
            }

            if (artist.AlbumArtists.Count != 0)
            {
                return new DeleteArtistResult
                {
                    Success = false,
                    ErrorMessage = $"Artist with ID {artistId} cannot be deleted because they are associated with {artist.AlbumArtists.Count} album(s).",
                };
            }

            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync(cancellationToken);

            return new DeleteArtistResult
            {
                Success = true,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete artist with ID {ArtistId}", artistId);
            return new DeleteArtistResult
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    private static ArtistDetails MapToArtistDetails(Artist artist) => new()
    {
        Id = artist.Id,
        Name = artist.Name,
        Bio = artist.Bio ?? string.Empty,
        CreatedAt = artist.CreatedAt,
        Albums = artist.AlbumArtists
            .Select(aa => new AlbumSummary
            {
                Id = aa.Album.Id,
                Title = aa.Album.Title,
                Artists = aa.Album.AlbumArtists
                    .Select(a => new ArtistSummary
                    {
                        Id = a.Artist.Id,
                        Name = a.Artist.Name,
                    })
                    .ToList(),
            })
            .ToList(),
    };
}