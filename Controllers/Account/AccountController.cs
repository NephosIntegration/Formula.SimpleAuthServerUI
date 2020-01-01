// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


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

namespace Formula.SimpleAuthServerUI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : SimpleAccountControllerBase
    {
        private AccountAuthService _authService;

        public AccountController(
            AppUserManager userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events)
        {
            _authService = new AccountAuthService(userManager, signInManager, schemeProvider, interaction, clientStore, events);
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await _authService.GetLoginDetailsAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDetails model, string button)
        {
            // the user clicked the "cancel" button
            if (button != "login")
            {
                var context = await _authService.CancelLoginAsync(model.ReturnUrl);
                return HandleReturnUrl(context, false);
            }

            var results = this.HandleModelState();

            if (results.IsSuccessful) 
            {
                results = await _authService.LoginAsync(model);
                var loginResults = results.GetDataAs<LoginResults>();

                if (results.IsSuccessful)
                {
                    if (loginResults.Result.RequiresTwoFactor)
                    {
                        return RedirectToAction("SendCode", "TwoFactorAuth", new { ReturnUrl = model.ReturnUrl, RememberMe = model.RememberLogin });
                    }
                    else
                    {
                        var context = await _authService.GetContextAsync(model.ReturnUrl);
                        return HandleReturnUrl(context);
                    }
                }
                else
                {

                    ModelState.AddModelError(string.Empty, results.Message);
                }
            }

            // something went wrong, show form with error
            var vm = await _authService.GetLoginDetailsAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return View(vm);
        }

        
        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await _authService.GetLogoutDetailsAsync(User, logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await _authService.GetPostLogoutDetailsAsync(HttpContext, User, model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                await _authService.SignOutAsync(User);
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            if (vm.PostLogoutRedirectUri != null) 
            {
                return HandleReturnUrl(URLTrustType.Native, vm.PostLogoutRedirectUri);
            }
            else
            {
                return View("LoggedOut", vm);
            }
        }

    }
}