using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherMvc.Services
{
    public interface ITokenService
    {
        Task<TokenResponse> GetCCToken(string scope);

        Task<TokenResponse> GetACFToken(string accesstoken);

        Task<TokenResponse> GetRefreshToken(string refreshtoken);

    }
}
