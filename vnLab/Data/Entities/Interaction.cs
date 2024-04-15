using vnLab.Data.Entities.Enum;

namespace vnLab.Data.Entities;

public class Interaction
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; }
    public string? UserId { get; set; }
    public User? User { get; set; }
    public InteractionType? InteractionType { get; set; }
    public int Rating { get; set; }
    public DateTime TimeStamp { get; set; }
}