using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vnLab.Data;
using vnLab.Data.Entities;
using vnLab.Models;

namespace vnLab.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly vnLabDbContext _context;
    private readonly UserManager<User> _userManager;


    public HomeController(ILogger<HomeController> logger, vnLabDbContext context, UserManager<User> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    [Route("my-posts")]
    public async Task<IActionResult> MyPosts(string currentFilter, string searchString, int? pageNumber)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        if (searchString != null)
        {
            pageNumber = 1;
        }
        else
        {
            searchString = currentFilter;
        }

        var posts = from m in _context.Posts select m;

        posts = posts.Where(x => x.UserId == currentUser.Id);

        if (!String.IsNullOrEmpty(searchString))
        {
            posts = posts.Where(s => s.Title!.Contains(searchString)
            || s.Title!.Contains(searchString)
            || s.Content!.Contains(searchString)
            || s.Tags!.Contains(searchString));
        }
        return View(PaginatedList<Post>.Create(await posts.OrderByDescending(x => x.Modified).ToListAsync(), pageNumber ?? 1, 20));
    }

    public async Task<IActionResult> PostByTag(string tag, int? pageNumber)
    {

        var posts = from m in _context.Posts select m;

        posts = posts.Where(x => x.Tags!.Contains(tag));

        return View(PaginatedList<Post>.Create(await posts.OrderByDescending(x => x.Modified).ToListAsync(), pageNumber ?? 1, 20));
    }

    public async Task<IActionResult> Index(string currentFilter, string searchString, int? pageNumber)
    {
        if (searchString != null)
        {
            pageNumber = 1;
        }
        else
        {
            searchString = currentFilter;
        }

        var posts = from m in _context.Posts select m;

        if (!String.IsNullOrEmpty(searchString))
        {
            posts = posts.Where(s => s.Title!.Contains(searchString)
            || s.Title!.Contains(searchString)
            || s.Content!.Contains(searchString)
            || s.Tags!.Contains(searchString));
        }
        return View(PaginatedList<Post>.Create(await posts.OrderByDescending(x => x.Modified).ToListAsync(), pageNumber ?? 1, 20));
    }

    public async Task<IActionResult> Admin(string currentFilter, string searchString, int? pageNumber)
    {
        if (searchString != null)
        {
            pageNumber = 1;
        }
        else
        {
            searchString = currentFilter;
        }

        var posts = from m in _context.Posts.Include(x => x.User) select m;

        if (!String.IsNullOrEmpty(searchString))
        {
            posts = posts.Where(s => s.Title!.Contains(searchString)
            || s.Title!.Contains(searchString)
            || s.Content!.Contains(searchString)
            || s.Tags!.Contains(searchString));
        }
        return View(PaginatedList<Post>.Create(await posts.OrderByDescending(x => x.Modified).ToListAsync(), pageNumber ?? 1, 20));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
