using Jukebox.DataAccess.EntityFramework.Models;
using Microsoft.EntityFrameworkCore;

namespace Jukebox.DataAccess.EntityFramework;

public class JukeboxDbContext : DbContext
{
    public JukeboxDbContext(DbContextOptions<JukeboxDbContext> options) : base(options) { }

    public DbSet<Song> Songs => Set<Song>();
    public DbSet<SongLyrics> SongLyrics => Set<SongLyrics>();
    public DbSet<AlbumDescription> AlbumDescriptions => Set<AlbumDescription>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<AlbumArtist> AlbumArtists => Set<AlbumArtist>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<SongGenre> SongGenres => Set<SongGenre>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Bio).HasMaxLength(2000);
            entity.HasMany(a => a.Songs).WithOne(s => s.Artist).HasForeignKey(s => s.ArtistId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Title).IsRequired().HasMaxLength(200);
            entity.HasMany(a => a.Songs).WithOne(s => s.Album).HasForeignKey(s => s.AlbumId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(a => a.Description)
                  .WithOne(d => d.Album)
                  .HasForeignKey<AlbumDescription>(d => d.AlbumId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AlbumArtist>(entity =>
        {
            entity.HasKey(aa => new { aa.AlbumId, aa.ArtistId });
            entity.HasOne(aa => aa.Album)
                  .WithMany(a => a.AlbumArtists)
                  .HasForeignKey(aa => aa.AlbumId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(aa => aa.Artist)
                  .WithMany()
                  .HasForeignKey(aa => aa.ArtistId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Song>(entity =>
         {
             entity.HasKey(s => s.Id);
             entity.Property(s => s.Title).IsRequired().HasMaxLength(200);
             entity.HasOne(s => s.Lyrics)
                   .WithOne(l => l.Song)
                   .HasForeignKey<SongLyrics>(l => l.SongId)
                   .OnDelete(DeleteBehavior.Cascade);
         });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
            entity.Property(g => g.Description).HasMaxLength(2000);

            entity.HasOne(g => g.ParentGenre)
                  .WithMany(g => g.SubGenres)
                  .HasForeignKey(g => g.ParentGenreId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SongGenre>(entity =>
        {
            entity.HasKey(sg => new { sg.SongId, sg.GenreId });
            entity.HasOne(sg => sg.Song)
                  .WithMany(s => s.SongGenres)
                  .HasForeignKey(sg => sg.SongId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(sg => sg.Genre)
                  .WithMany(g => g.SongGenres)
                  .HasForeignKey(sg => sg.GenreId);
        });
    }
}
