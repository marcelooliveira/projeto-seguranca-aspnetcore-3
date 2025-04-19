using MedVoll.Web.Data;
using MedVoll.Web.Filters;
using MedVoll.Web.Interfaces;
using MedVoll.Web.Repositories;
using MedVoll.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ExceptionHandlerFilter>();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ExceptionHandlerFilter>();
});

var connectionString = builder.Configuration.GetConnectionString("SqliteConnection");
builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlite(connectionString));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "VollMedAuthCookie"; // Nome do cookie
    options.LoginPath = "/Account/Login"; // Redireciona para login se não autenticado
    options.LogoutPath = "/Account/Logout"; // Caminho para logout
    options.AccessDeniedPath = "/Account/AccessDenied"; // Caminho para acesso negado
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5); // Tempo de expiração
    options.SlidingExpiration = true; // Renova o cookie automaticamente

    options.Cookie.HttpOnly = true; // Impede acesso via JavaScript
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Exige HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Restringe envio de cookies entre sites
});


var uri = new Uri(builder.Configuration["ApiUrl"]);
HttpClient httpClient = new HttpClient()
{
    BaseAddress = uri
};

builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<IMedicoRepository, MedicoRepository>();
builder.Services.AddTransient<IConsultaRepository, ConsultaRepository>();
builder.Services.AddTransient<IMedicoService, MedicoService>();
builder.Services.AddTransient<IConsultaService, ConsultaService>();
builder.Services.AddTransient<IMedVollApiService, MedVollApiService>();

builder.Services.AddSingleton(typeof(HttpClient), httpClient);

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true; // Proteger cookie contra acesso via JavaScript
    options.Cookie.IsEssential = true; // Garantir que o cookie seja salvo mesmo sem consentimento do usuário (GDPR)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Exigir HTTPS para cookies
    options.IdleTimeout = TimeSpan.FromMinutes(1); // Tempo de expiração da sessão
});

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddRazorPages();

const long ExpireInMinutes = 360;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
    .AddCookie("Cookies", options => options.ExpireTimeSpan = TimeSpan.FromMinutes(ExpireInMinutes))
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

app.UseSession();

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
