using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Formula.SimpleAuthServerUI
{
    public class OpenIdConnectServerDetails 
    {
        public String Name { get; set; }
        public Action<OpenIdConnectOptions> Options { get; set; }
    }

    public interface IOpenIdConnectConfig
    {
        List<OpenIdConnectServerDetails> GetExternalOpenIdConnectServers();
    }
}