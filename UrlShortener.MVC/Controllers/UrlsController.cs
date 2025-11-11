using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UrlShortener.MVC.Commons;
using UrlShortener.MVC.Data;
using UrlShortener.MVC.Data.Entities;
using UrlShortener.MVC.Models;
using QRCoder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace UrlShortener.MVC.Controllers
{
    [Authorize]
    public class UrlsController : Controller
    {
        private readonly UrlShortenerDbContext _context;
        private readonly ILogger<UrlsController> _logger;
        private readonly LinkGenerator _linkGenerator;

        [ActivatorUtilitiesConstructor]
        public UrlsController(UrlShortenerDbContext context, ILogger<UrlsController> logger, LinkGenerator linkGenerator)
        {
            _context = context;
            _logger = logger;
            _linkGenerator = linkGenerator; 
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

            var scheme = HttpContext?.Request?.Scheme ?? "https";
            var host = HttpContext?.Request?.Host.Value ?? string.Empty;
            ViewData["Host"] = string.IsNullOrEmpty(host) ? string.Empty : $"{scheme}://{host}/r/";

            return PartialView(url);
        }

        // GET: /Urls/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Urls/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UrlVM urlVM)
        {
            int maxShortCodeLength = AppConstant.ShortCodeLength;
            urlVM.ShortCode = NormalizeShortCode(urlVM.ShortCode);
            if (string.IsNullOrWhiteSpace(urlVM.ShortCode))
                urlVM.ShortCode = await GenerateUniqueShortCodeAsync(maxShortCodeLength);

            // System-managed fields
            urlVM.CreatedAt = DateTime.UtcNow;
            urlVM.ClickCount = 0;

            var  userId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            urlVM.UserId = userId;
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

                return RedirectToAction(nameof(Create));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Create Url failed");
                ModelState.AddModelError(string.Empty, "Colud not save URL. Try a different short code.");
                return View(urlVM);
            }
        }

        [HttpGet("/r/{code}", Name = "ShortRedirect")]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            var item = await _context.Urls.FirstOrDefaultAsync(x => x.ShortCode == code);

            if (item == null)
            {
                return NotFound("Short Url not found");
            }

            try
            {
                item.ClickCount += 1;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to increment click count for {Code}", code);
            }

            // Ensure OriginalUrl is absolute. Try to prepend https if missing.
            if (!Uri.TryCreate(item.OriginalUrl, UriKind.Absolute, out var target))
            {
                if (!Uri.TryCreate("https://" + item.OriginalUrl, UriKind.Absolute, out target))
                {
                    _logger.LogWarning("Stored OriginalUrl invalid for {Code}: {Original}", code, item.OriginalUrl);
                    return BadRequest("The stored original URL is invalid.");
                }
            }

            return Redirect(target.ToString());
        }

        // GET: /r/qrcode/{code}  -> returns PNG image of QR for the short URL
        [HttpGet("/r/qrcode/{code}")]
        public async Task<IActionResult> QrCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("Short code is required.");

            code = NormalizeShortCode(code);

            var item = await _context.Urls.FirstOrDefaultAsync(x => x.ShortCode == code);
            if (item == null)
                return NotFound("Short Url not found.");

            // Use the stored OriginalUrl directly — simpler.
            var original = (item.OriginalUrl ?? string.Empty).Trim();

            // Ensure we generate a valid absolute URI. Try prepending https if missing.
            if (!Uri.TryCreate(original, UriKind.Absolute, out var target))
            {
                if (!Uri.TryCreate("https://" + original, UriKind.Absolute, out target))
                {
                    _logger.LogWarning("Invalid OriginalUrl for code {Code}: {Original}", code, item.OriginalUrl);
                    return BadRequest("The stored original URL is invalid.");
                }
            }

            return GenerateQrCode(target.ToString());
        }

        public IActionResult GenerateQrCode(string url)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            using var qrCodeImage = qrCode.GetGraphic(20);
            using var ms = new System.IO.MemoryStream();
            qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return File(ms.ToArray(), "image/png");
        }


        // GET: /Urls/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var url = await _context.Urls.FindAsync(id);
            if (url == null) return NotFound();

            var scheme = HttpContext?.Request?.Scheme ?? "https";
            var host = HttpContext?.Request?.Host.Value ?? string.Empty;
            ViewData["Host"] = string.IsNullOrEmpty(host) ? string.Empty : $"{scheme}://{host}/r/";

            return PartialView(url);
        }

        // POST: /Urls/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UrlVM urlVM)
        {
            bool isOK = false;
            string message = "";
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

            // Apply allowed changes (protect CreatedAt, ClickCount, and UserId)        
            dbUrl.OriginalUrl = urlVM.OriginalUrl;
            dbUrl.ShortCode = string.IsNullOrWhiteSpace(normalized) ? dbUrl.ShortCode : normalized;
            try
            {
                isOK = true;
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Create));
                return Json(new { isOK = isOK, message = message });
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
                return PartialView(dbUrl);
            }
        }

        // GET: /Urls/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var url = await _context.Urls.FirstOrDefaultAsync(m => m.Id == id);
            if (url == null) return NotFound();

            return PartialView(url);
        }

        // POST: /Urls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool isOK = false;
            string message = "";
            var url = await _context.Urls.FindAsync(id);
            if (url != null)
            {
                _context.Urls.Remove(url);
                await _context.SaveChangesAsync();
                isOK = true;
                message = "Url deleted successfully.";
            }
            else
            {
                message = "Url not found.";
            }
            return Json(new { isOK = isOK, message = message });
        }

        // Helpers
        private Task<bool> UrlExists(int id) =>
            _context.Urls.AnyAsync(e => e.Id == id);

        private static string NormalizeShortCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return string.Empty;
            var trimmed = code.Trim();
            // Allow letters (both cases), digits, dash and underscore — preserve case
            return new string(trimmed
                .Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')
                .ToArray());
        }

        private async Task<string> GenerateUniqueShortCodeAsync(int length)
        {
            // include uppercase letters so generated codes can contain capitals
            const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rnd = Random.Shared;

            // ensure length is within allowed bounds
            length = Math.Clamp(length, 3, MaxLengths.ShortCode);

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

        public IActionResult Reload()
        {
            return ViewComponent("UrlList");
        }



    }
}