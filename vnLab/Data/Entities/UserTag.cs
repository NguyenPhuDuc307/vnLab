namespace vnLab.Data.Entities;

public class UserTag
{
    public string? UserId { get; set; }
    public User? User { get; set; }
    public string? TagId { get; set; }
    public Tag? Tag { get; set; }
    public int Number { get; set; }
}