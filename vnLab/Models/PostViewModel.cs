namespace vnLab.Models
{
    public class PostViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime Asked { get; set; }
        public List<string>? Tags { get; set; }
    }
}