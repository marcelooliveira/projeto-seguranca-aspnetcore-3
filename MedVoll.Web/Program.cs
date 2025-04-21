using MedVoll.Web.Filters;
using MedVoll.Web.Interfaces;
using MedVoll.Web.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ExceptionHandlerFilter>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ExceptionHandlerFilter>();
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "VollMedAuthCookie"; // Nome do cookie
    options.LoginPath = builder.Configuration["IdentityUrl"] + "/Account/Login"; // Redireciona para login se não autenticado
    options.LogoutPath = builder.Configuration["IdentityUrl"] + "/Account/Logout"; // Caminho para logout
    options.AccessDeniedPath = builder.Configuration["IdentityUrl"] + "/Account/AccessDenied"; // Caminho para acesso negado
    options.ExpireTimeSpan = TimeSpan.FromMinutes(1); // Tempo de expiração
    options.SlidingExpiration = true; // Renova o cookie automaticamente

    options.Cookie.HttpOnly = true; // Impede acesso via JavaScript
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Exige HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Restringe envio de cookies entre sites
});


var uri = new Uri(builder.Configuration["MedVoll.WebApi.Url"]);
HttpClient httpClient = new HttpClient()
{
    BaseAddress = uri
};

builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<IMedVollApiService, MedVollApiService>();

builder.Services.AddSingleton(typeof(HttpClient), httpClient);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddRazorPages();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
    .AddCookie("Cookies")
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://localhost:5001";

        options.ClientId = "MedVoll.Web";
        options.ClientSecret = "secret";
        options.ResponseType = "code";

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("MedVoll.WebAPI");

        options.GetClaimsFromUserInfoEndpoint = true;

        options.MapInboundClaims = false; // Don't rename claim types

        options.SaveTokens = true;
    });


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/erro/500");
    app.UseStatusCodePagesWithReExecute("/erro/{0}");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
