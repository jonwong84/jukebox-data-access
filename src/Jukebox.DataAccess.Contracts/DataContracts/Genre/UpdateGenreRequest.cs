namespace Jukebox.DataAccess.Contracts.DataContracts.Genre;

public class UpdateGenreRequest
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? ParentGenreId { get; set; }
    public string? UserId { get; set; }
}