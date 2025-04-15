using Duende.IdentityModel.Client;
using System.Net.Http;

namespace MedVoll.Web.Services
{
    public class SessionHelper : ISessionHelper
    {
        private readonly IHttpContextAccessor contextAccessor;
        private readonly HttpClient _httpClient;

        public IConfiguration Configuration { get; }

        public SessionHelper(IHttpContextAccessor contextAccessor, IConfiguration configuration, HttpClient httpClient)
        {
            this.contextAccessor = contextAccessor;
            Configuration = configuration;
            _httpClient = httpClient;
        }

        public int? GetPedidoId()
        {
            return contextAccessor.HttpContext.Session.GetInt32("pedidoId");
        }

        public void SetPedidoId(int pedidoId)
        {
            contextAccessor.HttpContext.Session.SetInt32("pedidoId", pedidoId);
        }

        public async Task<string> GetAccessToken(string scope)
        {
            //var tokenClient = new TokenClient(Configuration["IdentityUrl"] + "connect/token", "MVC", "secret");

            //            var tokenResponse = await tokenClient.RequestClientCredentialsAsync(scope);
            //return tokenResponse.AccessToken;

            var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = Configuration["IdentityUrl"] + "connect/token",
                ClientId = "MedVoll.Web",
                ClientSecret = "secret",
                Scope = scope
            };

            var response = await _httpClient.RequestClientCredentialsTokenAsync(tokenRequest);

            if (response.IsError)
            {
                throw new Exception($"Token request failed: {response.Error}");
            }

            return response.AccessToken;
        }

        public void SetAccessToken(string accessToken)
        {
            contextAccessor.HttpContext.Session.SetString("accessToken", accessToken);
        }
    }
}
