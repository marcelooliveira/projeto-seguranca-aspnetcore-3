using MedVoll.Web.Dtos;
using MedVoll.Web.Exceptions;
using MedVoll.Web.Interfaces;
using MedVoll.Web.Models;
using MedVoll.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedVoll.Web.Controllers
{
    [Authorize()]
    [Route("medicos")]
    public class MedicoController : BaseController
    {
        private const string PaginaListagem = "Listagem";
        private const string PaginaCadastro = "Formulario";
        private readonly IMedicoService _service;
        private readonly IMedVollApiService _medVollApiService;

        public MedicoController(IMedVollApiService medVollApiService)
        : base()
        {
            _medVollApiService = medVollApiService;
        }

        [HttpGet]
        [Route("{page?}")]
        public async Task<IActionResult> ListarAsync([FromQuery] int page = 1)
        {
            var medicos = await _medVollApiService.WithHttpContext(HttpContext).ListarMedicos(page);
            ViewBag.Consultas = medicos;
            ViewData["Url"] = "Medicos";
            return View(PaginaListagem, medicos);
        }

        [HttpGet]
        [Route("formulario/{id?}")]
        public async Task<IActionResult> ObterFormularioAsync(long? id = 0)
        {
            MedicoDto medico = await _medVollApiService.WithHttpContext(HttpContext).ObterFormularioMedico(id);
            return View(PaginaCadastro, medico);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> SalvarAsync([FromForm] MedicoDto dados)
        {
            if (dados._method == "delete")
            {
                await _medVollApiService.WithHttpContext(HttpContext).ExcluirMedico(dados.Id.Value);
                return Redirect("/medicos");
            }

            if (!ModelState.IsValid)
            {
                IEnumerable<MedicoDto> medicos = await _medVollApiService.WithHttpContext(HttpContext).ListarMedicos(1);
                ViewData["Medicos"] = medicos.ToList();
                return View(PaginaCadastro, dados);
            }

            try
            {
                await _medVollApiService.WithHttpContext(HttpContext).CadastrarMedico(dados);
                return Redirect("/medicos");
            }
            catch (RegraDeNegocioException ex)
            {
                ViewBag.Erro = ex.Message;
                ViewBag.Dados = dados;
                return View(PaginaCadastro);
            }
        }

        [HttpGet]
        [Route("especialidade/{especialidade}")]
        public async Task<IActionResult> ListarPorEspecialidadeAsync(string especialidade)
        {
            if (Enum.TryParse(especialidade, out Especialidade especEnum))
            {
                var medicos = await _medVollApiService.WithHttpContext(HttpContext).ListarMedicosPorEspecialidade(especEnum);
                return Json(medicos);
            }
            return BadRequest("Especialidade inválida.");
        }
    }
}
