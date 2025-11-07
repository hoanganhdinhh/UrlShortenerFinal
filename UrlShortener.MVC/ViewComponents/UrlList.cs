using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.MVC.Data;
using UrlShortener.MVC.Models;

namespace UrlShortener.MVC.ViewComponents
{
    public class UrlListViewComponent : ViewComponent
    {
        private readonly UrlShortenerDbContext _context;
        public UrlListViewComponent(UrlShortenerDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var urlEntities = await _context.Urls
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            var viewModels = urlEntities.Select(u => new UrlVM
            {
                Id = u.Id,
                OriginalUrl = u.OriginalUrl,
                ShortCode = u.ShortCode,
                CreatedAt = u.CreatedAt,
                // ClickCount not present on entity; default 0
            }).ToList();

            return View("Default", viewModels);
        }
    }
}
