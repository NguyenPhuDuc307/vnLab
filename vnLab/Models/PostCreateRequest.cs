namespace vnLab.Models;

public class PostCreateRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime Asked { get; set; }
    public string[]? Tags { get; set; }
}