using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TwitterWannaBE.Models;

public class Post
{
    public int Id { get; set; }

    [Required]
    [StringLength(140, ErrorMessage = "El mensaje no puede tener más de 140 caracteres")]
    public string Content { get; set; } = string.Empty;

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public bool Edited { get; set; } = false;

    public DateTime? EditedAt { get; set; }

    [Required]
    public string AuthorId { get; set; } = string.Empty;

    public IdentityUser? Author { get; set; }
}
