using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;
using System.Security.Claims;

namespace IdentityServer.Data
{
    public static class Config
    {
        // add scopes to access token 
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
        {
                new IdentityResources.OpenId(),
                //new IdentityResources.Email(),
                new IdentityResources.Profile(),
                /*new IdentityResources.Phone(),
                new IdentityResources.Address(),
                */
                // should be requested at openId client config
                new IdentityResource()
                {
                    Name="roles",
                    DisplayName="Roles",
                    Description= "User roles",
                    UserClaims = { JwtClaimTypes.Role, "Permission" },
                },
                // add custom claim to the Token, should be in user claims as well
                new IdentityResource()
                {
                    Name= "OurClaim",
                    UserClaims = { "My_Claim" }

                }
        };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope(
                        "OnlineStore.create",
                        displayName: "Write your data.",
                        userClaims: new[] { "user_level" }
                    ),
                new ApiScope("OnlineStore.read"),              
                new ApiScope("OnlineStore.update"),
            };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("OnlineStore")
            {
                Scopes = new List<string> { "OnlineStore.read", "OnlineStore.update"},
                //ApiSecrets = new List<Secret> {new Secret("ScopeSecret".Sha256())},
                // avaiable user claims returned to client after auth 
                // located inside User.Claims object
                // add claim to access_tokens
                UserClaims = new List<string> { 
                    /*"role",
                    JwtClaimTypes.Email,
                    JwtClaimTypes.EmailVerified,
                    JwtClaimTypes.PhoneNumber,
                    JwtClaimTypes.PhoneNumberVerified,
                    JwtClaimTypes.GivenName,
                    JwtClaimTypes.FamilyName,*/
                    JwtClaimTypes.PreferredUserName,
                    "role"
                }
            }
        };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
            // m2m client credentials flow client
                new Client
                {
                    ClientId = "m2m.client",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
                    // purpose of token
                    AllowedScopes = { "OnlineStore.read", "OnlineStore.update" }
                },

                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "interactive",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "https://localhost:7238/signin-oidc", "urn:ietf:wg:oauth:2.0:oob" },
                    FrontChannelLogoutUri = "https://localhost:7238/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:7238/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    // the AllowedScopes (claims) returned to the client in the access token
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "OnlineStore.read",
                        "OurClaim",
                        "roles",
                        "fullAccess",
                        "OnlineStore.update"
                    },
                    RequireConsent = true,
                    AlwaysIncludeUserClaimsInIdToken= true,
                },
                new Client
                {
                    ClientId = "angular",
                    RequireClientSecret = false,

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RedirectUris = { "http://localhost:4200" },
                    FrontChannelLogoutUri = "http://localhost:4200",
                    PostLogoutRedirectUris = { "http://localhost:4200" },
                    AllowedCorsOrigins = { "http://localhost:4200" },
                    AccessTokenLifetime = 600,
                    IdentityTokenLifetime = 600,
                    AllowOfflineAccess = true,
                    AllowedScopes = {
                        //IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        //IdentityServerConstants.StandardScopes.Phone,
                        //IdentityServerConstants.StandardScopes.Address,
                        //IdentityServerConstants.StandardScopes.OfflineAccess,
                        //"roles",
                        //"OnlineStore.create",
                        "OnlineStore.read",
                        //"OnlineStore.update",
                        //"OnlineStore.delete",
                        "OurClaim",
                        "roles",
                    },
                    RequireConsent = false,                        
                    AllowAccessTokensViaBrowser = true,

                    AlwaysIncludeUserClaimsInIdToken= true,
                }
            };
    }
}
