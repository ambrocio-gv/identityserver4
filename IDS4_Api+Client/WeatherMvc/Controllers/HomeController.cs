using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WeatherMvc.Models;
using WeatherMvc.Services;

namespace WeatherMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITokenService _tokenService;
        private readonly IHttpClientFactory _httpClientFactory;
        //private readonly IOptions<IdentityServerSettings> _identityServerSettings;

        public HomeController(ILogger<HomeController> logger, ITokenService tokenService, IHttpClientFactory httpClientFactory, IOptions<IdentityServerSettings> identityServerSettings)
        {
            _logger = logger;
            _tokenService = tokenService;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        //for Client Credential Flow
        //[Authorize]
        //public async Task<IActionResult> Weather()
        //{
        //    var data = new List<WeatherData>();

        //    using (var client = new HttpClient())
        //    {
        //        var tokenResponse = await _tokenService.GetCCToken("weatherapi.read");

        //        client.SetBearerToken(tokenResponse.AccessToken); 

        //        var result = client //HttpClient
        //            .GetAsync("https://localhost:5445/weatherforecast")//Task<HttpResponseMessage>
        //            .Result;

        //        if (result.IsSuccessStatusCode)
        //        {
        //            var model = result.Content.ReadAsStringAsync().Result;

        //            data = JsonConvert.DeserializeObject<List<WeatherData>>(model);

        //            return View(data);
        //        }

        //        else
        //        {
        //            throw new Exception("Unable to get content");
        //        }
        //    }    
        //}

        [Authorize]
        public async Task<IActionResult> Weather(string code_verifier,
            string grant_type, // flow of access_token request
            string code, // confirmation of the authentication process
            string redirect_uri
            )
        {
            var data = new List<WeatherData>();

            //var code = await HttpContext.Request.QueryString
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var idToken = await HttpContext.GetTokenAsync("id_token");
            //var refreshToken = await HttpContext.GetTokenAsync("refresh_token");

            var _accessToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var _idToken = new JwtSecurityTokenHandler().ReadJwtToken(idToken);

            using (var client = new HttpClient())
            {
                //var tokenResponse = await _tokenService.GetCCToken("weatherapi.read");

                //var tokenResponse = await _tokenService.GetACFToken(accessToken);


                client.SetBearerToken(accessToken);

                await RefreshAccessToken();

                var result = client //HttpClient
                    .GetAsync("https://localhost:5445/weatherforecast")//Task<HttpResponseMessage>
                    .Result;

                if (result.IsSuccessStatusCode)
                {
                    var model = result.Content.ReadAsStringAsync().Result;

                    data = JsonConvert.DeserializeObject<List<WeatherData>>(model);

                    return View(data);
                }

                else
                {
                    throw new Exception("Unable to get content");
                }
            }


            //return View();

        }

        private async Task RefreshAccessToken()
        {
            var serverClient = _httpClientFactory.CreateClient();
            //var discoveryDocument = await serverClient.GetDiscoveryDocumentAsync("https://localhost:44305/");

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var idToken = await HttpContext.GetTokenAsync("id_token");
            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
            var refreshTokenClient = _httpClientFactory.CreateClient();


            var tokenResponse = await _tokenService.GetRefreshToken(refreshToken);


            //var tokenResponse = await refreshTokenClient.RequestRefreshTokenAsync(
            //    new RefreshTokenRequest {
            //        Address = discoveryDocument.TokenEndpoint,
            //        RefreshToken = refreshToken,
            //        //ClientId = _identityServerSettings.Value.ClientName,
            //        ClientId = "interactive",

            //        //ClientSecret = _identityServerSettings.Value.ClientPassword
            //        ClientSecret = "SuperSecretPassword"
            //    });

            var authInfo = await HttpContext.AuthenticateAsync("cookie");

            authInfo.Properties.UpdateTokenValue("access_token", tokenResponse.AccessToken);
            authInfo.Properties.UpdateTokenValue("id_token", tokenResponse.IdentityToken);
            authInfo.Properties.UpdateTokenValue("refresh_token", tokenResponse.RefreshToken);

            await HttpContext.SignInAsync("cookie", authInfo.Principal, authInfo.Properties);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
