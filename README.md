# Formula.SimpleAuthServer
A simple OAuth2 / OpenID Connect User Interface for the Authorization Server wrapper for Identity Server

## Enable GUI
To in enable a gui uncomment two sections in the Startup.cs
- **ConfigureServices** - services.AddSimpleAuthServerUI();
- **Configure** - app.UseSimpleAuthServerUI();

## Defining Authentication Options
You will need to specify OpenID Connect options to specify the authorization server to use.

This can be done by creating your own class that implements the IOpenIdConnectConfig contract.

This can be passed as a parameter to AddSimpleAuthServerUI.

For demo / testing purposes, an example JWT Bearer will be set up (see OpenIdConnectConfigDemo for details).

# Relavant Links
- [IdentityServer4 Docs](https://identityserver4.readthedocs.io)

# Packages / Projects Used
- [IdentityServer4](https://www.nuget.org/packages/IdentityServer4/)
