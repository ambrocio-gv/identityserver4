using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherMvc.Services
{
    public class InteractiveServiceSettings
    {
        public string AuthorityUrl { get; set; }
        public string ClientId{ get; set; }
        public string ClientSecret { get; set; }
        public string Response_type { get; set; }
        public string[] Scopes { get; set; }
        public bool UseHttps { get; set; }
    }
}
