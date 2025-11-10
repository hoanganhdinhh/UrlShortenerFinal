using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using UrlShortener.MVC.Data.Entities.Identities;
using UrlShortener.MVC.Models.Identities;
using UrlShortener.Services.Mail.Mailjet;
using UrlShortener.Services.Otp;

namespace UrlShortener.MVC.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly SignInManager<UrlShortenerUser> _signInManager;
        private readonly UserManager<UrlShortenerUser> _userManager;
        private readonly IUserStore<UrlShortenerUser> _userStore;
        private readonly IUserEmailStore<UrlShortenerUser> _emailStore;
        //private readonly ILogger<LoginModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IOtpService _otpService;

        public AuthenticationController(SignInManager<UrlShortenerUser> signInManager,
            UserManager<UrlShortenerUser> userManager,
            IUserStore<UrlShortenerUser> userStore,
            IEmailSender emailSender,
            IOtpService otpService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _emailSender = emailSender;
            _otpService = otpService;
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
        public async Task<IActionResult> Register(RegisterVM model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new UrlShortenerUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            // === Dùng OTP để xác nhận email ===
            const string purpose = "confirm-email";
            var otp = await _otpService.GenerateAndReturnCodeAsync(model.Email, purpose);

            await _emailSender.SendEmailAsync(
                model.Email,
                "Your verification code",
                $@"<p>Thanks for registering!</p>
           <p>Your verification code is:</p>
           <h2 style='letter-spacing:4px'>{otp}</h2>
           <p>This code expires in 10 minutes.</p>");

            // Điều hướng sang trang nhập OTP
            return RedirectToAction(nameof(VerifyOtp), new { email = model.Email, returnUrl });
        }



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Register(RegisterVM model, string? returnUrl = null)
        //{
        //    if (!ModelState.IsValid) return View(model);

        //    var user = new UrlShortenerUser { UserName = model.Email, Email = model.Email };
        //    var result = await _userManager.CreateAsync(user, model.Password);
        //    if (!result.Succeeded)
        //    {
        //        foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
        //        return View(model);
        //    }

        //    // Tạo token + link xác nhận
        //    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //    var codeEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        //    var callbackUrl = Url.Action(
        //        action: "ConfirmEmail",
        //        controller: "Authentication",
        //        values: new { userId = user.Id, code = codeEncoded, returnUrl },
        //        protocol: Request.Scheme)!;

        //    // Gửi mail
        //    await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
        //        $"Thank you for registering an account with us. We are excited to have you on board.<br><br>To complete your registration, please confirm your email address by clicking the link below: <br><br><a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Click Here</a><br><br>If you did not request this account, please disregard this email.<br><br>Best regards");

        //    // Chuyển tới trang thông báo
        //    return RedirectToAction(nameof(RegisterConfirmation), new { email = model.Email, returnUrl });
        //}

        [HttpGet]
        public async Task<IActionResult> RegisterConfirmation(string email, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(email)) return RedirectToAction(nameof(Login));

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return NotFound($"Unable to load user with email '{email}'.");

            // Dev mode: hiển thị sẵn link xác nhận (tùy chọn)
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var codeEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var confirmUrl = Url.Action("ConfirmEmail", "Authentication",
                new { userId = user.Id, code = codeEncoded, returnUrl }, Request.Scheme);

            var vm = new RegisterConfirmationVM
            {
                Email = email,
                ReturnUrl = returnUrl,
                DisplayConfirmAccountLink = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development",
                UserId = user.Id,
                Code = codeEncoded
            };
            return View(vm);
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

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string? userId, string? code, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound($"Unable to load user with ID '{userId}'.");

            var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, decodedCode);

            if (result.Succeeded)
            {
                // (tuỳ chọn) tự động đăng nhập sau khi xác nhận
                // await _signInManager.SignInAsync(user, isPersistent: false);

                // Tránh lỗi khi returnUrl null hoặc không local
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                TempData["StatusMessage"] = "Cảm ơn bạn đã xác nhận email.";
                return RedirectToAction(nameof(Login));
            }

            TempData["StatusMessage"] = "Có lỗi khi xác nhận email.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email, string? returnUrl = null)
        {
            var verifyotpvm = new VerifyOtpVM
            {
                Email = email,
                Purpose = "confirm-email",
                ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? Url.Content("~/") : returnUrl
            };
            return View(verifyotpvm); // Views/Authentication/VerifyOtp.cshtml
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpVM verifyotpvm)
        {
            if (!ModelState.IsValid) return View(verifyotpvm);

            if (!_otpService.Verify(verifyotpvm.Email, verifyotpvm.Purpose, verifyotpvm.Otp))
            {
                ModelState.AddModelError(string.Empty, "Invalid or expired code. Please try again.");
                return View(verifyotpvm);
            }

            // OTP OK -> Confirm email trong Identity
            var user = await _userManager.FindByEmailAsync(verifyotpvm.Email);
            if (user == null) return NotFound();

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                // (tuỳ chọn) đăng nhập luôn:
                // await _signInManager.SignInAsync(user, isPersistent:false);
                return LocalRedirect(verifyotpvm.ReturnUrl);
            }

            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return View(verifyotpvm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email, string purpose = "confirm-email", string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return RedirectToAction(nameof(Login));

            if (!_otpService.CanResend(email, purpose))
            {
                TempData["ErrorMessage"] = "Please wait before requesting another code.";
                return RedirectToAction(nameof(VerifyOtp), new { email, returnUrl });
            }

            var otp = await _otpService.GenerateAndReturnCodeAsync(email, purpose);
            await _emailSender.SendEmailAsync(email, "Your verification code",
                $"Your new code is <b>{otp}</b>. It expires in 10 minutes.");

            TempData["Resent"] = true;
            return RedirectToAction(nameof(VerifyOtp), new { email, returnUrl });
        }


        //// Gửi lại email xác nhận
        //[HttpGet]
        //public async Task<IActionResult> ResendConfirmation(string email, string? returnUrl = null)
        //{
        //    if (string.IsNullOrWhiteSpace(email))
        //        return RedirectToAction(nameof(Login));

        //    var user = await _userManager.FindByEmailAsync(email);
        //    if (user is null) return RedirectToAction(nameof(Login));

        //    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //    var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        //    var callbackUrl = Url.Action(
        //        action: "ConfirmEmail",
        //        controller: "Authentication",
        //        values: new { userId = user.Id, code },
        //        protocol: Request.Scheme);

        //    await _emailSender.SendEmailAsync(email,
        //        "Confirm your email",
        //        $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

        //    TempData["Resent"] = true;
        //    return RedirectToAction(nameof(RegisterConfirmation), new { email, returnUrl });
        //}

        //[HttpGet]
        //public async Task<IActionResult> ConfirmEmail(string userId, string code, string? returnUrl = null)
        //{
        //    if (userId == null || code == null) return BadRequest();

        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null) return NotFound();

        //    var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        //    var result = await _userManager.ConfirmEmailAsync(user, decoded);

        //    if (result.Succeeded) return Redirect(returnUrl ?? Url.Content("~/"));
        //    foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
        //    return View("Error");
        //}


        public IActionResult ExternalLogin()
        {
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Authentication", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        // GET: Callback from external provider (based on OnGetCallbackAsync)
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                TempData["ErrorMessage"] = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["ErrorMessage"] = "Error loading external login information.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            // Try sign-in with the external login info
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                TempData["ErrorMessage"] = "User account locked out.";
                return RedirectToAction(nameof(Login));
            }

            // No local account linked to this external login -> attempt auto-provision
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "External provider did not supply an email address.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            // Check if a user already exists with this email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Create a new local user (no password)
                user = CreateUser();
                await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, email, CancellationToken.None);

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    TempData["ErrorMessage"] = "Failed to create local user account.";
                    return RedirectToAction(nameof(Login), new { returnUrl });
                }
            }

            // Link the external login to the local user
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to link external login to local account.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            // Optionally mark email as confirmed when provisioning from a trusted provider
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["StatusMessage"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(); // Views/Authentication/ForgotPassword.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // Generate OTP code for password reset
            const string purpose = "reset-password";
            var otp = await _otpService.GenerateAndReturnCodeAsync(email, purpose);

            // Send OTP via email
            await _emailSender.SendEmailAsync(
                email,
                "Your Password Reset Code",
                $@"<p>You requested to reset your password.</p>
           <p>Your OTP code is:</p>
           <h2 style='letter-spacing:4px'>{otp}</h2>
           <p>This code expires in 10 minutes.</p>");

            return RedirectToAction(nameof(ForgotPasswordConfirmation), new { email });
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation(string email)
        {
            ViewBag.Email = email;
            return View(); // Views/Authentication/ForgotPasswordConfirmation.cshtml
        }

        [HttpGet]
        public IActionResult VerifyOtpForPasswordReset(string email)
        {
            var vm = new VerifyOtpVM
            {
                Email = email,
                Purpose = "reset-password",
                ReturnUrl = Url.Content("~/")
            };
            return View(vm); // Views/Authentication/VerifyOtp.cshtml
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> VerifyOtpForPasswordReset(VerifyOtpVM vm)
        //{
        //    if (!ModelState.IsValid) return View(vm);

        //    if (!_otpService.Verify(vm.Email, vm.Purpose, vm.Otp))
        //    {
        //        ModelState.AddModelError(string.Empty, "Invalid or expired code. Please try again.");
        //        return View(vm);
        //    }

        //    // OTP verified successfully, redirect to Reset Password page
        //    return RedirectToAction(nameof(ResetPassword), new { email = vm.Email });
        //}

        //[HttpGet]
        //public IActionResult ResetPassword(string email)
        //{
        //    var model = new ResetPasswordViewModel
        //    {
        //        Email = email
        //    };
        //    return View(model); // Views/Authentication/ResetPassword.cshtml
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        //{
        //    if (!ModelState.IsValid) return View(model);

        //    var user = await _userManager.FindByEmailAsync(model.Email);
        //    if (user == null)
        //    {
        //        return RedirectToAction(nameof(ForgotPasswordConfirmation));
        //    }

        //    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

        //    if (result.Succeeded)
        //    {
        //        return RedirectToAction(nameof(ResetPasswordConfirmation));
        //    }

        //    foreach (var error in result.Errors)
        //    {
        //        ModelState.AddModelError(string.Empty, error.Description);
        //    }

        //    return View(model);
        //}

        //[HttpGet]
        //public IActionResult ResetPasswordConfirmation()
        //{
        //    return View(); // Views/Authentication/ResetPasswordConfirmation.cshtml



    }
}
