using System.Diagnostics;
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


    public HomeController(ILogger<HomeController> logger, vnLabDbContext context)
    {
        _logger = logger;
        _context = context;
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
        return View(PaginatedList<Post>.Create(await posts.ToListAsync(), pageNumber ?? 1, 20));
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
