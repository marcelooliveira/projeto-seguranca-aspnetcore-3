using MedVoll.Web.Dtos;
using MedVoll.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MedVoll.Web.Controllers
{
    [Authorize]
    [Route("consultas")]
    public class ConsultaController : BaseController
    {
        private const string PaginaListagem = "Listagem";
        private const string PaginaCadastro = "Formulario";

        private readonly HttpClient _httpClient;
        private readonly JwtTokenHandler _jwtTokenHandler;
        private readonly IConfiguration _configuration;

        public ConsultaController(IHttpClientFactory httpClientFactory,
                                   JwtTokenHandler tokenHandler,
                                   IConfiguration configuration,
                                   SignInManager<IdentityUser> signInManager)
            : base(signInManager)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _jwtTokenHandler = tokenHandler;
            _configuration = configuration;
        }

        [HttpGet("{page?}")]
        public async Task<IActionResult> ListarAsync([FromQuery] int page = 1)
        {
            var token = _jwtTokenHandler.GetToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"https://localhost:7100/api/Consulta/listar?page={page}");

            if (!response.IsSuccessStatusCode)
            {
                return View(PaginaListagem);
            }

            var json = await response.Content.ReadAsStringAsync();
            var consultas = JsonSerializer.Deserialize<PaginatedList<ConsultaDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ViewData["Url"] = "Consultas";
            return View(PaginaListagem, consultas);
        }

        [HttpGet("formulario/{id?}")]
        public async Task<IActionResult> ObterFormularioAsync(long? id)
        {
            var token = _jwtTokenHandler.GetToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            ConsultaDto dados;

            if (id.HasValue)
            {
                var response = await _httpClient.GetAsync($"https://localhost:7100/api/Consulta/formulario/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Listar");
                }
                var json = await response.Content.ReadAsStringAsync();
                dados = JsonSerializer.Deserialize<ConsultaDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                dados = new ConsultaDto { Data = DateTime.Now };
            }

            var medicosResponse = await _httpClient.GetAsync("https://localhost:7100/api/Medico/listar");
            var medicosJson = await medicosResponse.Content.ReadAsStringAsync();
            var medicos = JsonSerializer.Deserialize<List<MedicoDto>>(medicosJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ViewData["Medicos"] = medicos;
            return View(PaginaCadastro, dados);
        }

        [HttpPost("")]
        public async Task<IActionResult> SalvarAsync([FromForm] ConsultaDto dados)
        {
            var token = _jwtTokenHandler.GetToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (dados._method == "delete")
            {
                if (dados.Id.HasValue)
                {
                    var response = await _httpClient.DeleteAsync($"https://localhost:7100/api/Consulta/Salvar?id={dados.Id.Value}");
                }
                return Redirect("/consultas");
            }

            try
            {
                var json = JsonSerializer.Serialize(dados);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://localhost:7100/api/Consulta/Salvar", content);

                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    ViewBag.Erro = erro;
                    return View(PaginaCadastro, dados);
                }

                return Redirect("/consultas");
            }
            catch (Exception ex)
            {
                ViewBag.Erro = ex.Message;
                return View(PaginaCadastro, dados);
            }
        }
    }
}
