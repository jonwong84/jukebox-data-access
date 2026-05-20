using Jukebox.DataAccess.Contracts.DataContracts.Song;
using Jukebox.DataAccess.EntityFramework.Models;
using Jukebox.DataAccess.Interfaces;

namespace Jukebox.DataAccess.Songs
{
    internal class SongRepository : ISongRepository
    {
        private readonly EntityFramework.JukeboxDbContext _context;

        public SongRepository(EntityFramework.JukeboxDbContext context)
        {
            _context = context;
        }

        public Task<AddSongResult> AddAsync(AddSongRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int songId)
        {
            throw new NotImplementedException();
        }

        public Task<Song?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(UpdateSongRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
