using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TwitterWannaBE.Data;
using TwitterWannaBE.Models;

namespace TwitterWannaBE.Pages.Posts;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public EditModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(140)]
        public string Content { get; set; } = string.Empty;
    }

    public bool EditAllowed { get; set; } = true;

    public string? WarningMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var post = await _db.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (post == null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null || post.AuthorId != user.Id)
            return Forbid();

        if ((DateTime.UtcNow - post.Created) > TimeSpan.FromMinutes(5))
        {
            EditAllowed = false;
            WarningMessage = "No se puede editar una publicación con más de 5 minutos de antigüedad.";
        }

        Input = new InputModel
        {
            Id = post.Id,
            Content = post.Content
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == Input.Id);
        if (post == null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null || post.AuthorId != user.Id)
            return Forbid();

        if ((DateTime.UtcNow - post.Created) > TimeSpan.FromMinutes(5))
        {
            ModelState.AddModelError(string.Empty, "No se puede editar una publicación con más de 5 minutos de antigüedad.");
            return Page();
        }

        post.Content = Input.Content;
        post.Edited = true;
        post.EditedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return RedirectToPage("/Index");
    }
}
