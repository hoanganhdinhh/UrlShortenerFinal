using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.MVC.Data;
using UrlShortener.MVC.Models;
using Microsoft.AspNetCore.Identity;
using UrlShortener.MVC.Data.Entities.Identities;

namespace UrlShortener.MVC.ViewComponents
{
    public class UrlListViewComponent : ViewComponent
    {
        private readonly UrlShortenerDbContext _context;
        private readonly UserManager<UrlShortenerUser> _userManager;

        public UrlListViewComponent(UrlShortenerDbContext context, UserManager<UrlShortenerUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get the current user's ID
            var userId = _userManager.GetUserId(HttpContext.User);

            // Filter URLs to only show the current user's URLs
            var urlEntities = await _context.Urls
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var viewModels = urlEntities.Select(u => new UrlVM
            {
                Id = u.Id,
                OriginalUrl = u.OriginalUrl,
                ShortCode = u.ShortCode,
                CreatedAt = u.CreatedAt,
                ClickCount = u.ClickCount,
            }).ToList();

            var scheme = HttpContext?.Request?.Scheme ?? "https";
            var host = HttpContext?.Request?.Host.Value ?? string.Empty;
            ViewData["Host"] = string.IsNullOrEmpty(host) ? string.Empty : $"{scheme}://{host}/r/";


            return View("Default", viewModels);
        }
    }
}
