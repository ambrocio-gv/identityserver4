using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Http;

namespace WeatherMvc.Services
{
    public class TokenService : ITokenService
    {

        private readonly ILogger<TokenService> _logger;
        private readonly IOptions<IdentityServerSettings> _identityServerSettings;
        private readonly IOptions<InteractiveServiceSettings> _interactiveServiceSettings;
        private readonly DiscoveryDocumentResponse _discoveryDocument;


        public TokenService(ILogger<TokenService> logger, IOptions<IdentityServerSettings> identityServerSettings, IOptions<InteractiveServiceSettings> interactiveServiceSettings)
        {
            _logger = logger;
            _identityServerSettings = identityServerSettings;
            _interactiveServiceSettings = interactiveServiceSettings;

            using var httpClient = new HttpClient();
            //_discoveryDocument = httpClient.GetDiscoveryDocumentAsync(identityServerSettings.Value.DiscoveryUrl).Result; //for Client Credentials flow
            _discoveryDocument = httpClient.GetDiscoveryDocumentAsync(interactiveServiceSettings.Value.AuthorityUrl).Result; //for Authorization Code flow


            if (_discoveryDocument.IsError)
            {
                logger.LogError($"Unable to get discovery document. Error is: {_discoveryDocument.Error}");
                throw new Exception("Unable to get discovery document", _discoveryDocument.Exception);
            }

        }


        //for client credentials flow 
        public async Task<TokenResponse> GetCCToken(string scope)
        {
            using var client = new HttpClient();
           
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest {
                
                Address = _discoveryDocument.TokenEndpoint,
                ClientId = _identityServerSettings.Value.ClientName,
                ClientSecret = _identityServerSettings.Value.ClientPassword,
                Scope = scope
            });


            if (tokenResponse.IsError)
            {
                _logger.LogError($"Unable to get token. Error is: {tokenResponse.Error}");
                throw new Exception("Unable to get token", tokenResponse.Exception);
            }

            return tokenResponse;
        }


        //for authorization code flow 

        public async Task<TokenResponse> GetACFToken(string accesstoken)
        {
            

            using var client = new HttpClient();

            var tokenResponse = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest {
                Address = _discoveryDocument.AuthorizeEndpoint,
                GrantType = "authorization_code",
                Code =  _interactiveServiceSettings.Value.Response_type,
                RedirectUri = "https://localhost:5444/signin-oidc", //callback path supposedly - this is optional
                ClientId = _interactiveServiceSettings.Value.ClientId,                
                ClientSecret = _interactiveServiceSettings.Value.ClientSecret
                

            });


            if (tokenResponse.IsError)
            {
                _logger.LogError($"Unable to get token. Error is: {tokenResponse.Error}");
                throw new Exception("Unable to get token", tokenResponse.Exception);
            }

            return tokenResponse;
        }

        public async Task<TokenResponse> GetRefreshToken(string refreshtoken)
        {
            using var client = new HttpClient();
            var tokenResponse = await client.RequestRefreshTokenAsync(new RefreshTokenRequest {
                Address = _discoveryDocument.TokenEndpoint,
                RefreshToken = refreshtoken,
                ClientId = _interactiveServiceSettings.Value.ClientId,
                ClientSecret = _interactiveServiceSettings.Value.ClientSecret,
                
            });

            if (tokenResponse.IsError)
            {
                _logger.LogError($"Unable to get refresh token. Error is: {tokenResponse.Error}");
                throw new Exception("Unable to get refresh token", tokenResponse.Exception);
            }

            return tokenResponse;
        }

    }
}
