//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.AspNetCore.Components.Server;
//using Microsoft.IdentityModel.JsonWebTokens;
//using System.Net;

//namespace MedVoll.Web.Extensions
//{
//    public static class Extensions
//    {
//        public static void AddAuthenticationServices(this IHostApplicationBuilder builder)
//        {
//            var configuration = builder.Configuration;
//            var services = builder.Services;

//            JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

//            var identityUrl = configuration["IdentityUrl"];
//            var callBackUrl = configuration["CallBackUrl"];
//            var sessionCookieLifetime = configuration.GetValue("SessionCookieLifetimeMinutes", 60);

//            // Add Authentication services
//            services.AddAuthorization();
//            services.AddAuthentication(options =>
//            {
//                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//                options.DefaultChallengeScheme = "oidc";
//                options.DefaultAuthenticateScheme = "Cookies";
//            })
//            .AddCookie(options => options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionCookieLifetime))
//            .AddOpenIdConnect("oidc", options =>
//             {
//                 options.Authority = "https://localhost:5100";

//                 options.ClientId = "MedVoll.Web";
//                 options.ClientSecret = "secret";
//                 options.ResponseType = "code";

//                 options.Scope.Clear();
//                 options.Scope.Add("openid");
//                 options.Scope.Add("profile");
//                 options.Scope.Add("MedVoll.WebAPI");
//                 options.GetClaimsFromUserInfoEndpoint = true;
//                 options.MapInboundClaims = false; // Don't rename claim types
//                 options.SaveTokens = true;
//             });

//            // Blazor auth services
//            services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
//            services.AddCascadingAuthenticationState();
//        }
//    }
//}
