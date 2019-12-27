using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Formula.SimpleMembership;
using Formula.SimpleAuthServer;

namespace Formula.SimpleAuthServerUI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class ExternalController : Controller
    {
        private ExternalAuthService _authService;

        public ExternalController(
            AppUserManager userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events)
        {
            _authService = new ExternalAuthService(userManager, signInManager, interaction, clientStore, events);
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Challenge(string provider, string returnUrl)
        {

            var urlCheck = _authService.ValidateReturnUrl(HttpContext, Url, returnUrl);

            if (urlCheck.IsSuccessful)
            {
                if (AccountOptions.WindowsAuthenticationSchemeName == provider)
                {
                    // windows authentication needs special handling
                    var result = await _authService.ProcessWindowsLoginAsync(HttpContext, Url, returnUrl);
                    if (result.IsSuccessful)
                    {
                        var redirectUri = result.GetDataAs<String>();
                        return Redirect(redirectUri);
                    }
                    else
                    {
                        // trigger windows auth
                        // since windows auth don't support the redirect uri,
                        // this URL is re-triggered when we call challenge
                        return Challenge(AccountOptions.WindowsAuthenticationSchemeName);
                    }
                }
                else
                {
                    // start challenge and roundtrip the return URL and scheme 
                    var props = new AuthenticationProperties
                    {
                        RedirectUri = Url.Action(nameof(Callback)),
                        Items =
                        {
                            { "returnUrl", returnUrl },
                            { "scheme", provider },
                        }
                    };

                    return Challenge(props, provider);
                }
            }
            else
            {
                throw new Exception(urlCheck.Message);
            }
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            var output = Redirect("~/"); // Default redirect is to the home page

            var results = await _authService.HandleExternalAuthAsync(HttpContext, Url);
            if (results.IsSuccessful)
            {
                // validate return URL and redirect back to authorization endpoint or a local page
                var returnUrl = results.GetDataAs<RedirectUrlDetails>();
                if (returnUrl.IsLocal)
                {
                    output = Redirect(returnUrl.Url);
                }
            }
            else
            {
                throw new Exception(results.Message);
            }

            return output;
        }

    }
}