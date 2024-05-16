using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vnLab.Data;

namespace vnLab.Controllers.Components;

public class TagViewComponent : ViewComponent
{
    private readonly vnLabDbContext _context;
    public TagViewComponent(vnLabDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var viewModel = await _context.PostTags
                .Include(pt => pt.Tag)
                .GroupBy(pt => pt.TagId)
                .OrderByDescending(g => g.Count())
                .Take(50)
                .Select(g => g.First())
                .ToListAsync();
        return View("Default", viewModel);
    }
}