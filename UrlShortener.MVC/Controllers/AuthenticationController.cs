using System.Text;
using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using UrlShortener.MVC.Data.Entities.Identities;
using UrlShortener.MVC.Models.Identities;

namespace UrlShortener.MVC.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly SignInManager<UrlShortenerUser> _signInManager;
        private readonly UserManager<UrlShortenerUser> _userManager;
        private readonly IUserStore<UrlShortenerUser> _userStore;
        private readonly IUserEmailStore<UrlShortenerUser> _emailStore;
        //private readonly ILogger<LoginModel> _logger;

        public AuthenticationController(SignInManager<UrlShortenerUser> signInManager,
            UserManager<UrlShortenerUser> userManager,
            IUserStore<UrlShortenerUser> userStore)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            var errorMessage = TempData["ErrorMessage"] as string;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                ModelState.AddModelError(string.Empty, errorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var loginVM = new LoginVM
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
            };

            return View(loginVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (ModelState.IsValid)
            {

                loginVM.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                if (ModelState.IsValid)
                {
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    var result = await _signInManager.PasswordSignInAsync(loginVM.Email, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        //_logger.LogInformation("User logged in.");
                        return LocalRedirect(loginVM.ReturnUrl);
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("/Authentication/LoginWith2fa", new { ReturnUrl = loginVM.ReturnUrl, RememberMe = loginVM.RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        //_logger.LogWarning("User account locked out.");
                        return RedirectToPage("/Authentication/Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(loginVM);
                    }
                }
            }
            return View(loginVM);
        }

        public async Task<IActionResult> Register(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var registerVM = new RegisterVM
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
            };
            return View(registerVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            registerVM.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, registerVM.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, registerVM.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, registerVM.Password);

                if (result.Succeeded)
                {
                    //_logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = registerVM.ReturnUrl },
                        protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(registerVM.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        //return RedirectToPage("RegisterConfirmation", new { email = registerVM.Email, returnUrl = registerVM.ReturnUrl });
                        return RedirectToAction("RegisterConfirmation", new { email = registerVM.Email, returnUrl = registerVM.ReturnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(registerVM.ReturnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(registerVM);
        }

        public async Task<IActionResult> RegisterConfirmation(string email, string returnUrl)
        {
            if (email == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            // Once you add a real email sender, you should remove this code that lets you confirm the account

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var model = new RegisterConfirmationVM
            {
                Email = email,
                UserId = userId,
                Code = code,
                //DisplayConfirmAccountLink = true,
                //EmailConfirmationUrl = Action(
                //    "/Account/ConfirmEmail",
                //    pageHandler: null,
                //    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                //    protocol: Request.Scheme),
                ReturnUrl = returnUrl
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterConfirmation(RegisterConfirmationVM registerConfirmationVM)
        {
            if (registerConfirmationVM.UserId == null || registerConfirmationVM.Code == null)
            {
                //return RedirectToPage("/Index");
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByIdAsync(registerConfirmationVM.UserId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{registerConfirmationVM.UserId}'.");
            }

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(registerConfirmationVM.Code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            var statusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            return LocalRedirect(registerConfirmationVM.ReturnUrl);
        }

        private UrlShortenerUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<UrlShortenerUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(UrlShortenerUser)}'. " +
                    $"Ensure that '{nameof(UrlShortenerUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
        private IUserEmailStore<UrlShortenerUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<UrlShortenerUser>)_userStore;
        }
    }
}
