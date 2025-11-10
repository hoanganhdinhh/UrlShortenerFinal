using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UrlShortener.MVC.Models;

namespace UrlShortener.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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
}



//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.Extensions.Logging;
//using UrlShortener.Services.Mail.Mailjet;

//namespace UrlShortener.MVC.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly ILogger<HomeController> _logger;
//        private readonly IEmailSender _emailSender; 

//        public HomeController(ILogger<HomeController> logger, IEmailSender emailSender)
//        {
//            _logger = logger;
//            _emailSender = emailSender;
//        }

//        public IActionResult Index() => View();

//        // /Home/TestMail -> gửi thử Mailjet
//        public async Task<IActionResult> TestMail()
//        {
//            await _emailSender.SendEmailAsync(
//                "dinhnguyenhoanganh2005@gmail.com",
//                "Test Mailjet",
//                "<b>Mailjet OK</b>"
//            );
//            return Content("Sent");
//        }
//        [HttpGet("/debug/mailjet")]
//        public IActionResult DebugMailjet([FromServices] Microsoft.Extensions.Options.IOptions<MailjetOptions> opt)
//        {
//            var o = opt.Value;
//            return Content($"ApiKey? {(string.IsNullOrEmpty(o.ApiKey) ? "MISSING" : "OK")}, " +
//                           $"FromEmail = '{o.FromEmail}'");
//        }
//    }
//}
