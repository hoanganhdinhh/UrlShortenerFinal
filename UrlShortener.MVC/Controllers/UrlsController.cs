using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UrlShortener.MVC.Data;
using UrlShortener.MVC.Data.Entities;
using UrlShortener.MVC.Models;

//namespace UrlShortener.MVC.Controllers
//{
//    public class UrlsController : Controller
//    {
//        private readonly UrlShortenerDbContext _context;

//        public UrlsController(UrlShortenerDbContext context)
//        {
//            _context = context;
//        }

//        // GET: Urls
//        public async Task<IActionResult> Index()
//        {
//            return View(await _context.Urls.ToListAsync());
//        }

//        // GET: Urls/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var url = await _context.Urls
//                .FirstOrDefaultAsync(m => m.Id == id);
//            if (url == null)
//            {
//                return NotFound();
//            }

//            return View(url);
//        }

//        // GET: Urls/Create
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // POST: Urls/Create
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("Id,OriginalUrl,ShortCode,ClickCount,CreatedAt,UserId")] Url url)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Add(url);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(url);
//        }

//        // GET: Urls/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var url = await _context.Urls.FindAsync(id);
//            if (url == null)
//            {
//                return NotFound();
//            }
//            return View(url);
//        }

//        // POST: Urls/Edit/5
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("Id,OriginalUrl,ShortCode,ClickCount,CreatedAt,UserId")] Url url)
//        {
//            if (id != url.Id)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    _context.Update(url);
//                    await _context.SaveChangesAsync();
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!UrlExists(url.Id))
//                    {
//                        return NotFound();
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                return RedirectToAction(nameof(Index));
//            }
//            return View(url);
//        }

//        // GET: Urls/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var url = await _context.Urls
//                .FirstOrDefaultAsync(m => m.Id == id);
//            if (url == null)
//            {
//                return NotFound();
//            }

//            return View(url);
//        }

//        // POST: Urls/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var url = await _context.Urls.FindAsync(id);
//            if (url != null)
//            {
//                _context.Urls.Remove(url);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        private bool UrlExists(int id)
//        {
//            return _context.Urls.Any(e => e.Id == id);
//        }
//    }
//}


//namespace URLShortener.Web.Controllers
//{
//    // [Authorize] // or [Authorize(Roles="Admin")]
//    public class UrlsController : Controller
//    {
//        private readonly UrlShortenerDbContext _context;
//        private readonly ILogger<UrlsController> _logger;

//        public UrlsController(UrlShortenerDbContext context, ILogger<UrlsController> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        // GET: /Urls
//        public async Task<IActionResult> Index()
//        {
//            var items = await _context.Urls
//                .OrderByDescending(u => u.CreatedAt)
//                .ToListAsync();
//            return View(items);
//        }

//        // GET: /Urls/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null) return NotFound();

//            var url = await _context.Urls.FirstOrDefaultAsync(m => m.Id == id);
//            if (url == null) return NotFound();

//            return View(url);
//        }

//        // GET: /Urls/Create
//        public IActionResult Create() => View();

//        // POST: /Urls/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create( UrlVM urlVM)
//        {
//            // If you track creator, attach from identity:
//            // urlVM.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

//            // Normalize & auto-generate if missing
//            urlVM.ShortCode = NormalizeShortCode(urlVM.ShortCode);
//            if (string.IsNullOrWhiteSpace(urlVM.ShortCode))
//                urlVM.ShortCode = await GenerateUniqueShortCodeAsync(6);

//            // System-managed fields
//            urlVM.CreatedAt = DateTime.UtcNow;
//            urlVM.ClickCount = 0;



//            // Uniqueness check (friendly validation error)
//            if (await _context.Urls.AnyAsync(u => u.ShortCode == urlVM.ShortCode))
//                ModelState.AddModelError(nameof(UrlVM.ShortCode), "Short code already exists. Please choose another.");

//            if (!ModelState.IsValid) return View(urlVM);

//            try
//            {
//                _context.Add(urlVM);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            catch (DbUpdateException ex)
//            {
//                _logger.LogError(ex, "Create Url failed");
//                ModelState.AddModelError(string.Empty, "Could not save URL. Try a different short code.");
//                return View(urlVM);
//            }
//        }

//        // GET: /Urls/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null) return NotFound();

//            var url = await _context.Urls.FindAsync(id);
//            if (url == null) return NotFound();

//            return View(url);
//        }

//        // POST: /Urls/Edit/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, UrlVM urlVM)
//        {
//            if (id != urlVM.Id) return NotFound();

//            var dbUrl = await _context.Urls.FirstOrDefaultAsync(u => u.Id == id);
//            if (dbUrl == null) return NotFound();

//            // Normalize & ensure uniqueness if changed
//            var normalized = NormalizeShortCode(urlVM.ShortCode);
//            if (!string.Equals(dbUrl.ShortCode, normalized, StringComparison.Ordinal))
//            {
//                if (await _context.Urls.AnyAsync(u => u.ShortCode == normalized && u.Id != id))
//                    ModelState.AddModelError(nameof(UrlVM.ShortCode), "Short code already exists. Please choose another.");
//            }

//            if (!ModelState.IsValid) return View(dbUrl);

//            // Apply allowed changes (protect CreatedAt & ClickCount)
//            dbUrl.OriginalUrl = urlVM.OriginalUrl;
//            dbUrl.ShortCode = string.IsNullOrWhiteSpace(normalized) ? dbUrl.ShortCode : normalized;
//            dbUrl.UserId = urlVM.UserId;

//            try
//            {
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            catch (DbUpdateConcurrencyException)
//            {
//                if (!await UrlExists(dbUrl.Id)) return NotFound();
//                throw;
//            }
//            catch (DbUpdateException ex)
//            {
//                _logger.LogError(ex, "Edit Url failed");
//                ModelState.AddModelError(string.Empty, "Could not update URL.");
//                return View(dbUrl);
//            }
//        }

//        // GET: /Urls/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null) return NotFound();

//            var url = await _context.Urls.FirstOrDefaultAsync(m => m.Id == id);
//            if (url == null) return NotFound();

//            return View(url);
//        }

//        // POST: /Urls/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var url = await _context.Urls.FindAsync(id);
//            if (url == null) return NotFound();

//            _context.Urls.Remove(url);
//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        // Helpers
//        private Task<bool> UrlExists(int id) =>
//            _context.Urls.AnyAsync(e => e.Id == id);

//        private static string NormalizeShortCode(string? code)
//        {
//            if (string.IsNullOrWhiteSpace(code)) return string.Empty;
//            var trimmed = code.Trim();
//            // keep URL-friendly chars; lowercase for consistency
//            return new string(trimmed
//                .Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')
//                .ToArray())
//                .ToLowerInvariant();
//        }

//        private async Task<string> GenerateUniqueShortCodeAsync(int length = 6)
//        {
//            const string alphabet = "abcdefghijkmnpqrstuvwxyz23456789"; // avoid 0/O, 1/l
//            var rnd = Random.Shared;

//            while (true)
//            {
//                var span = new char[length];
//                for (int i = 0; i < length; i++)
//                    span[i] = alphabet[rnd.Next(alphabet.Length)];
//                var candidate = new string(span);

//                var exists = await _context.Urls.AnyAsync(u => u.ShortCode == candidate);
//                if (!exists) return candidate;
//            }
//        }
//    }
//}



namespace URLShortener.Web.Controllers
{
    // [Authorize] // or [Authorize(Roles="Admin")]
    public class UrlsController : Controller
    {
        private readonly UrlShortenerDbContext _context;
        private readonly ILogger<UrlsController> _logger;

        public UrlsController(UrlShortenerDbContext context, ILogger<UrlsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Urls
        public async Task<IActionResult> Index()
        {
            var items = await _context.Urls
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            return View(items);
        }

        // GET: /Urls/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var url = await _context.Urls.FirstOrDefaultAsync(m => m.Id == id);
            if (url == null) return NotFound();

            return View(url);
        }

        // GET: /Urls/Create
        public IActionResult Create() => View();

        // POST: /Urls/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UrlVM urlVM)
        {
            // If you track creator, attach from identity:
            // urlVM.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Normalize & auto-generate if missing
            urlVM.ShortCode = NormalizeShortCode(urlVM.ShortCode);
            if (string.IsNullOrWhiteSpace(urlVM.ShortCode))
                urlVM.ShortCode = await GenerateUniqueShortCodeAsync(6);

            // System-managed fields
            urlVM.CreatedAt = DateTime.UtcNow;
            urlVM.ClickCount = 0;

            // Uniqueness check (friendly validation error)
            if (await _context.Urls.AnyAsync(u => u.ShortCode == urlVM.ShortCode))
                ModelState.AddModelError(nameof(UrlVM.ShortCode), "Short code already exists. Please choose another.");

            if (!ModelState.IsValid) return View(urlVM);

            try
            {
                // Map view model to EF entity before adding
                var entity = new Url
                {
                    OriginalUrl = urlVM.OriginalUrl,
                    ShortCode = urlVM.ShortCode,
                    ClickCount = urlVM.ClickCount,
                    CreatedAt = urlVM.CreatedAt,
                    UserId = urlVM.UserId
                };

                _context.Urls.Add(entity);
                await _context.SaveChangesAsync();

                // Optionally set the generated Id back to the VM (if the view uses it after redirect)
                urlVM.Id = entity.Id;

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Create Url failed");
                ModelState.AddModelError(string.Empty, "Colud not save URL. Try a different short code.");
                return View(urlVM);
            }
        }

        // GET: /Urls/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var url = await _context.Urls.FindAsync(id);
            if (url == null) return NotFound();

            return View(url);
        }

        // POST: /Urls/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UrlVM urlVM)
        {
            if (id != urlVM.Id) return NotFound();

            var dbUrl = await _context.Urls.FirstOrDefaultAsync(u => u.Id == id);
            if (dbUrl == null) return NotFound();

            // Normalize & ensure uniqueness if changed
            var normalized = NormalizeShortCode(urlVM.ShortCode);
            if (!string.Equals(dbUrl.ShortCode, normalized, StringComparison.Ordinal))
            {
                if (await _context.Urls.AnyAsync(u => u.ShortCode == normalized && u.Id != id))
                    ModelState.AddModelError(nameof(UrlVM.ShortCode), "Short code already exists. Please choose another.");
            }

            if (!ModelState.IsValid) return View(dbUrl);

            // Apply allowed changes (protect CreatedAt & ClickCount)
            dbUrl.OriginalUrl = urlVM.OriginalUrl;
            dbUrl.ShortCode = string.IsNullOrWhiteSpace(normalized) ? dbUrl.ShortCode : normalized;
            dbUrl.UserId = urlVM.UserId;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UrlExists(dbUrl.Id)) return NotFound();
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Edit Url failed");
                ModelState.AddModelError(string.Empty, "Could not update URL.");
                return View(dbUrl);
            }
        }

        // GET: /Urls/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var url = await _context.Urls.FirstOrDefaultAsync(m => m.Id == id);
            if (url == null) return NotFound();

            return View(url);
        }

        // POST: /Urls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var url = await _context.Urls.FindAsync(id);
            if (url == null) return NotFound();

            _context.Urls.Remove(url);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helpers
        private Task<bool> UrlExists(int id) =>
            _context.Urls.AnyAsync(e => e.Id == id);

        private static string NormalizeShortCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return string.Empty;
            var trimmed = code.Trim();
            // keep URL-friendly chars; lowercase for consistency
            return new string(trimmed
                .Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')
                .ToArray())
                .ToLowerInvariant();
        }

        private async Task<string> GenerateUniqueShortCodeAsync(int length = 6)
        {
            const string alphabet = "abcdefghijkmnpqrstuvwxyz23456789"; // avoid 0/O, 1/l
            var rnd = Random.Shared;

            while (true)
            {
                var span = new char[length];
                for (int i = 0; i < length; i++)
                    span[i] = alphabet[rnd.Next(alphabet.Length)];
                var candidate = new string(span);

                var exists = await _context.Urls.AnyAsync(u => u.ShortCode == candidate);
                if (!exists) return candidate;
            }
        }
    }
}