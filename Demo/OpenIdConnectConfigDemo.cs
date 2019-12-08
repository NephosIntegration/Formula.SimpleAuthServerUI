using System;
using System.Collections.Generic;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Formula.SimpleAuthServerUI
{
    public class OpenIdConnectConfigDemo : IOpenIdConnectConfig
    {

        public List<OpenIdConnectServerDetails> GetExternalOpenIdConnectServers()
        {
            var output = new List<OpenIdConnectServerDetails>();

            output.Add(new OpenIdConnectServerDetails() {
                Name = "This Server (looped)",
                Options = new Action<OpenIdConnectOptions>( options => {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;

                    options.ClientId = "OpenIDConnectDemo";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";

                    options.SaveTokens = true;
                })
            });

            output.Add(new OpenIdConnectServerDetails() {
                Name = "Remote Cloud Server",
                Options = new Action<OpenIdConnectOptions>( options => {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                    options.SaveTokens = true;

                    options.Authority = "https://demo.identityserver.io/";
                    options.ClientId = "native.code";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                })
            });

            return output;
        }

        public static OpenIdConnectConfigDemo Get() 
        {
            return new OpenIdConnectConfigDemo();
        }
    }
}