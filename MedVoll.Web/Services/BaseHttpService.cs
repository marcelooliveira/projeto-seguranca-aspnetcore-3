using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace MedVoll.Web.Services
{
    delegate Task<HttpResponseMessage> HttpVerbMethod(Uri requestUri, HttpContent content);

    public abstract class BaseHttpService : IService, IBaseHttpService
    {
        protected readonly IConfiguration _configuration;
        protected readonly HttpClient _httpClient;
        protected readonly ISessionHelper _sessionHelper;
        protected string _baseUri;
        protected HttpContext _httpContext;

        public BaseHttpService(IConfiguration configuration, HttpClient httpClient, ISessionHelper sessionHelper)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _sessionHelper = sessionHelper;
        }

        public abstract string Scope { get; }

        protected async Task<T> GetAuthenticatedAsync<T>(string uri, params object[] param)
        {
            await SetToken();
            return await GetAsync<T>(uri, param);
        }

        protected async Task<T> GetAsync<T>(string uri, params object[] param)
        {
            string requestUri =
                string.Format(new Uri(new Uri(_baseUri), uri).ToString(), param);

            foreach (var par in param)
            {
                requestUri += string.Format($"/{par}");
            }

            var json = await _httpClient.GetStringAsync(requestUri);
            return JsonConvert.DeserializeObject<T>(json);
        }

        protected async Task<T> PostAsync<T>(string uri, object content)
        {
            HttpVerbMethod httpVerbMethod = new HttpVerbMethod(_httpClient.PostAsync);
            return await PutOrPostAsync<T>(uri, content, httpVerbMethod);
        }

        protected async Task<T> PutAsync<T>(string uri, object content)
        {
            HttpVerbMethod httpVerbMethod = new HttpVerbMethod(_httpClient.PutAsync);
            return await PutOrPostAsync<T>(uri, content, httpVerbMethod);
        }

        protected async Task DeleteAsync<T>(string uri, params object[] param)
        {
            string requestUri =
                string.Format(new Uri(new Uri(_baseUri), uri).ToString(), param);

            foreach (var par in param)
            {
                requestUri += string.Format($"/{par}");
            }

            var json = await _httpClient.DeleteAsync(requestUri);
        }

        private async Task<T> PutOrPostAsync<T>(string uri, object content, HttpVerbMethod httpVerbMethod)
        {
            var jsonIn = JsonConvert.SerializeObject(content);
            var stringContent = new StringContent(jsonIn, Encoding.UTF8, "application/json");

            await SetToken();

            HttpResponseMessage httpResponse = await httpVerbMethod(new Uri(new Uri(_baseUri), uri), stringContent);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorContent);
            }
            var jsonOut = await httpResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(jsonOut);
        }

        private async Task SetToken()
        {
            var accessToken = await _httpContext.GetTokenAsync("access_token");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}
