namespace MedVoll.Web.Services
{
    public class JwtTokenHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtTokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void StoreToken(string token)
        {
            _httpContextAccessor.HttpContext.Session.SetString("JWToken", token);
        }

        public string GetToken()
        {
            return _httpContextAccessor.HttpContext.Session.GetString("JWToken") ?? string.Empty;
        }
    }

}
