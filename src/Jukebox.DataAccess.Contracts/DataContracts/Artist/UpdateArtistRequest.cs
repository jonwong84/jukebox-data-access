namespace Jukebox.DataAccess.Contracts.DataContracts.Artist;

public class UpdateArtistRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? UserId { get; set; }
}