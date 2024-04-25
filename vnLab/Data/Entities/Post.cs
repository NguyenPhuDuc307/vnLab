using System.ComponentModel.DataAnnotations;

namespace vnLab.Data.Entities;

public class Post
{
    public int Id { get; set; }
    [Display(Name = "Tiêu đề")]
    public string? Title { get; set; }
    [Display(Name = "Nội dung")]
    public string? Content { get; set; }
    [Display(Name = "Ngày đăng")]
    public DateTime Asked { get; set; }
    [Display(Name = "Ngày chỉnh sửa")]
    public DateTime Modified { get; set; }
    [Display(Name = "Lượt xem")]
    public int Viewed { get; set; }
    [Display(Name = "Nhãn dán")]
    public string? Tags { get; set; }
    public string? UserId { get; set; }
    public User? User { get; set; }
    public ICollection<PostTag>? PostTags { get; set; }
}