using System.Collections.Generic;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityService.Configuration.Clients
{
    public class ClientData
    {
        public static IEnumerable<Client> GetClients()
        {
            var clientDev = ClientFactory(clientId: "authcodeflowclient_dev", clientName: "student3 - client.webapi.se");

            clientDev.ClientSecrets = new List<Secret> { new Secret("mysecret".Sha256()) };

            clientDev.RedirectUris = new List<string>()
            {
                "https://localhost:5001/signin-oidc",
                "https://localhost:5002/signin-oidc",
                "https://localhost:8001/authcode/callback"
            };

            clientDev.PostLogoutRedirectUris = new List<string>()
            {
                "https://localhost:5001/signout-callback-oidc"
            };

            clientDev.FrontChannelLogoutUri = "https://localhost:5001/signout-oidc";

            clientDev.AllowedCorsOrigins = new List<string>()
            {
                "https://localhost:5001"
            };
            clientDev.AllowedScopes.Add("idresource1");
            clientDev.AllowedScopes.Add("apiscope1");

            //Define the production client
            var clientProd = ClientFactory(clientId: "authcodeflowclient_prod", clientName: "student3-client.webapi.se");

            clientProd.ClientSecrets = new List<Secret> { new Secret("mysecret".Sha256()) };

            clientProd.RedirectUris = new List<string>()
            {
                "https://student3-client.webapi.se/signin-oidc"
            };

            clientProd.PostLogoutRedirectUris = new List<string>()
            {
                "https://student3-client.webapi.se/signout-callback-oidc"
            };

            clientProd.FrontChannelLogoutUri = "https://student3-client.webapi.se/signout-oidc";

            clientProd.AllowedCorsOrigins = new List<string>()
            {
                "https://student3-client.webapi.se"
            };
            return new List<Client>()
            {
                clientDev,
                clientProd
            };
        }
        private static Client ClientFactory(string clientId, string clientName)
        {
            return new Client()
            {
                ClientId = clientId,
                ClientName = clientName,
                ClientUri = "https://www.edument.se",
                RequirePkce = true,
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                AllowOfflineAccess = true,

                // When requesting both an id token and access token, should the user claims always
                // be added to the id token instead of requiring the client to use the UserInfo endpoint.
                // Defaults to false.
                AlwaysIncludeUserClaimsInIdToken = false,

                //Specifies whether this client is allowed to receive access tokens via the browser. 
                //This is useful to harden flows that allow multiple response types 
                //(e.g. by disallowing a hybrid flow client that is supposed to  use code id_token to add the token response type and thus leaking the token to the browser.
                AllowAccessTokensViaBrowser = false,

                AllowedScopes =
                    {
                        //Standard scopes
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Profile,
                        "employee",
                        "payment",
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                    },

                AlwaysSendClientClaims = true,
                ClientClaimsPrefix = "client_",
            };
        }
    }
}
