using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using IdentityServer4;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;

namespace Formula.SimpleAuthServerUI
{
    public static class SimpleAuthServerUIConfiguration
    {
        public static IServiceCollection AddSimpleAuthServerUI(this IServiceCollection services, AuthenticationBuilder authenticationBuilder)
        {
            // Enable all the log in / log out / consent etc.. Views
            services.AddControllersWithViews();
            
            // We will change the claim map, so don't use the default
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // The views will handle the authentication via cookies
            authenticationBuilder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            return services;
        }

        public static IApplicationBuilder UseSimpleAuthServerUI(this IApplicationBuilder app) {
            
            return app.UseStaticFiles();
        }

    }
}
