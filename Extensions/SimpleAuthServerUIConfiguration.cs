using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using IdentityServer4;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Formula.SimpleAuthServerUI
{
    public static class SimpleAuthServerUIConfiguration
    {
        public static IServiceCollection AddSimpleAuthServerUI(this IServiceCollection services)
        {
            services.AddControllersWithViews();
            
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            
            var builder = services.AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            return services;
        }

        public static IApplicationBuilder UseSimpleAuthServerUI(this IApplicationBuilder app) {
            
            return app.UseStaticFiles();
        }

    }
}
