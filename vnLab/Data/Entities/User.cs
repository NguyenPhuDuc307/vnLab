using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace vnLab.Data.Entities;

public class User : IdentityUser
{
    [MaxLength(50)]
    [Required]
    public string? FullName { get; set; }
    public ICollection<UserTag>? UserTags { get; set; }
}