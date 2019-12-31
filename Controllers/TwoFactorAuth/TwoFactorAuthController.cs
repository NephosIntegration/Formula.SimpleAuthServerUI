using Formula.SimpleMembership;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Formula.SimpleAuthServer;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Formula.SimpleAPI;

namespace Formula.SimpleAuthServerUI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class TwoFactorAuthController : SimpleControllerBase
    {
        private TwoFactorAuthService _authService;

        public TwoFactorAuthController(
            AppUserManager userManager,
            SignInManager<ApplicationUser> signInManager,
            UrlEncoder urlEncoder,
            IEmailSender emailSender,
            ISmsSender smsSender)
        {
            _authService = new TwoFactorAuthService(userManager, signInManager, urlEncoder, emailSender, smsSender);
        }

 
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        //
        // GET: /TwoFactorAuth/SendCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        {
            IActionResult output = View("Error");

            var results = await _authService.GetOptions(returnUrl, rememberMe);

            if (results.IsSuccessful)
            {
                var model = results.GetDataAs<SendCodeViewModel>();
                output = View(model);
            }

            return output;
        }

        //
        // POST: /TwoFactorAuth/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(SendCodeViewModel model)
        {
            IActionResult output = View();
            if (ModelState.IsValid)
            {
                var results = await _authService.Validate2faUser();
                if (results.IsSuccessful == false)
                {
                    output = View("Error");
                }
                else
                {
                    if (model.SelectedProvider == "Authenticator")
                    {
                        output = RedirectToAction(nameof(VerifyAuthenticatorCode), new { ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
                    }
                    else
                    {
                        var user = results.GetDataAs<ApplicationUser>();

                        results = await _authService.SendToken(user, model);
                        if (results.IsSuccessful)
                        {
                            output = RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
                        }
                        else
                        {
                            output = View("Error");
                        }
                    }
                }
            }

            return output;
        }

        //
        // GET: /TwoFactorAuth/VerifyCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
        {
            IActionResult output = View("Error");

            // Require that the user has already logged in via username/password or external login
            var results = await _authService.Validate2faUser();
            if (results.IsSuccessful)
            {
                output = View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
            }

            return output;
        }

        //
        // POST: /TwoFactorAuth/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _authService.TwoFactorSignInAsync(model);
            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                //_logger.LogWarning(7, "User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return View(model);
            }
        }

        //
        // GET: /TwoFactorAuth/VerifyAuthenticatorCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyAuthenticatorCode(bool rememberMe, string returnUrl = null)
        {
            IActionResult output = View("Error");

            // Require that the user has already logged in via username/password or external login
            var results = await _authService.Validate2faUser();
            if (results.IsSuccessful)
            {
                output = View(new VerifyAuthenticatorCodeViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe });
            }

            return output;
        }

        //
        // POST: /TwoFactorAuth/VerifyAuthenticatorCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAuthenticatorCode(VerifyAuthenticatorCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _authService.TwoFaLogin(model);
            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                //_logger.LogWarning(7, "User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return View(model);
            }

        }

        //
        // GET: /TwoFactorAuth/UseRecoveryCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> UseRecoveryCode(string returnUrl = null)
        {
            IActionResult output = View("Error");

            // Require that the user has already logged in via username/password or external login
            var results = await _authService.Validate2faUser();
            if (results.IsSuccessful)
            {
                output = View(new UseRecoveryCodeViewModel { ReturnUrl = returnUrl });
            }

            return output;
        }

        //
        // POST: /TwoFactorAuth/UseRecoveryCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseRecoveryCode(UseRecoveryCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.TwoFaRecovery(model);
            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return View(model);
            }
        }
    }
}