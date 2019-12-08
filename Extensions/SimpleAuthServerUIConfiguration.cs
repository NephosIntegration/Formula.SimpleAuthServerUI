using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;

namespace Formula.SimpleAuthServerUI
{
    public static class SimpleAuthServerUIConfiguration
    {
        public static IServiceCollection AddSimpleAuthServerUI(this IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddAuthentication();

            return services;
        }
    }
}
