using MedVoll.Web.Dtos;
using MedVoll.Web.Models;
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
    [Route("medicos")]
    public class MedicoController : BaseController
    {
        private const string PaginaListagem = "Listagem";
        private const string PaginaCadastro = "Formulario";
        private readonly HttpClient _httpClient;
        private readonly JwtTokenHandler _jwtTokenHandler;
        private readonly IConfiguration _config;

        public MedicoController(
            IHttpClientFactory httpClientFactory,
            JwtTokenHandler jwtTokenHandler,
            IConfiguration config,
            SignInManager<IdentityUser> signInManager)
            : base(signInManager)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _jwtTokenHandler = jwtTokenHandler;
            _config = config;
        }

        [HttpGet("{page?}")]
        public async Task<IActionResult> ListarAsync([FromQuery] int page = 1)
        {
            var token = _jwtTokenHandler.GetToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"https://localhost:7100/api/Medico/Listar?page={page}");

            if (!response.IsSuccessStatusCode)
            {
                return View(PaginaListagem);
            }

            var content = await response.Content.ReadAsStringAsync();
            var medicos = JsonSerializer.Deserialize<PaginatedList<MedicoDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            ViewData["Url"] = "Medicos";
            return View(PaginaListagem, medicos);
        }

        [HttpGet("formulario/{id?}")]
        public async Task<IActionResult> ObterFormularioAsync(long? id)
        {
            var token = _jwtTokenHandler.GetToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            MedicoDto dados;
            if (id.HasValue)
            {
                var response = await _httpClient.GetAsync($"https://localhost:7100/api/Medico/formulario/{id.Value}");
                if (!response.IsSuccessStatusCode) return NotFound();
                var json = await response.Content.ReadAsStringAsync();
                dados = JsonSerializer.Deserialize<MedicoDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                dados = new MedicoDto();
            }

            return View(PaginaCadastro, dados);
        }

        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [HttpPost("")]
        public async Task<IActionResult> SalvarAsync([FromForm] MedicoDto dados)
        {
            var token = _jwtTokenHandler.GetToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (dados._method == "delete")
            {
                var deleteRequest = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7100/api/Medico")
                {
                    Content = new StringContent(JsonSerializer.Serialize(dados), Encoding.UTF8, "application/json")
                };
                deleteRequest.Headers.Add("X-HTTP-Method-Override", "DELETE");
                await _httpClient.SendAsync(deleteRequest);
                return Redirect("/medicos");
            }

            if (!ModelState.IsValid)
            {
                return View(PaginaCadastro, dados);
            }

            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(dados), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://localhost:7100/api/Medico", jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    ViewBag.Erro = erro;
                    return View(PaginaCadastro, dados);
                }

                return Redirect("/medicos");
            }
            catch (Exception ex)
            {
                ViewBag.Erro = ex.Message;
                return View(PaginaCadastro, dados);
            }
        }

        [HttpGet("especialidade/{especialidade}")]
        public async Task<IActionResult> ListarPorEspecialidadeAsync(string especialidade)
        {
            var token = _jwtTokenHandler.GetToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (Enum.TryParse(especialidade, out Especialidade especEnum))
            {
                var response = await _httpClient.GetAsync($"https://localhost:7100/api/Medico/especialidade/{especEnum}");
                if (!response.IsSuccessStatusCode) return BadRequest("Erro ao buscar médicos");

                var json = await response.Content.ReadAsStringAsync();
                var medicos = JsonSerializer.Deserialize<List<MedicoDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Json(medicos);
            }

            return BadRequest("Especialidade inválida.");
        }
    }
}
