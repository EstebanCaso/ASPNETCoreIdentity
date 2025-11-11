using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TwitterWannaBE.Data;
using TwitterWannaBE.Models;

namespace TwitterWannaBE.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    // Feed size (choose between 15 and 50 as required)
    private const int FeedSize = 25;

    public IndexModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public List<Post> Posts { get; set; } = new();

    [BindProperty]
    public CreateInput? Input { get; set; }

    public class CreateInput
    {
        [Required]
        [StringLength(140, ErrorMessage = "El mensaje no puede tener más de 140 caracteres")]
        public string Content { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        Posts = await _db.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.Created)
            .Take(FeedSize)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            // redirect to login if not authenticated
            return Challenge();
        }

        if (Input == null)
        {
            return RedirectToPage();
        }

        if (!ModelState.IsValid)
        {
            // reload posts so the page can render errors inline
            await OnGetAsync();
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        var post = new Post
        {
            Content = Input.Content,
            AuthorId = user.Id,
            Created = DateTime.UtcNow
        };

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        return RedirectToPage();
    }
}
