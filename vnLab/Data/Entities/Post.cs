namespace vnLab.Data.Entities;

public class Post
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime Asked { get; set; }
    public DateTime Modified { get; set; }
    public int Viewed { get; set; }
    public string? Tags { get; set; }
    public string? UserId { get; set; }
    public User? User { get; set; }
    public ICollection<PostTag>? PostTags { get; set; }
}