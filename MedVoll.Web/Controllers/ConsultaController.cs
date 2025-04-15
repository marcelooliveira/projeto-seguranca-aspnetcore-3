using MedVoll.Web.Dtos;
using MedVoll.Web.Exceptions;
using MedVoll.Web.Services;
using Microsoft.AspNetCore.Authentication;

//using MedVoll.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MedVoll.Web.Controllers
{
    [Authorize]
    [Route("consultas")]
    public class ConsultaController : BaseController
    {
        private const string PaginaListagem = "Listagem";
        private const string PaginaCadastro = "Formulario";

        private readonly IMedVollApiService _medVollApiService;
        //private readonly IMedicoService _medicoService;

        public ConsultaController(IMedVollApiService medVollApiService)
        : base()
        {
            _medVollApiService = medVollApiService;
        }

        [HttpGet]
        [Route("{page?}")]
        public async Task<IActionResult> ListarAsync([FromQuery] int page = 1)
        {
            var consultas = await _medVollApiService.WithHttpContext(HttpContext).ListarConsultas(page);
            ViewBag.Consultas = consultas;
            ViewData["Url"] = "Consultas";
            return View(PaginaListagem, consultas);
        }

        [HttpGet]
        [Route("formulario/{id?}")]
        public async Task<IActionResult> ObterFormularioAsync(long id = 0)
        {
            FormularioConsultaDto formularioConsulta = await _medVollApiService.WithHttpContext(HttpContext).ObterFormularioConsulta(id);
            ViewData["Medicos"] = formularioConsulta.Medicos;
            return View(PaginaCadastro, formularioConsulta.Consulta);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> SalvarAsync([FromForm] ConsultaDto dados)
        {
            if (dados._method == "delete")
            {
                await _medVollApiService.WithHttpContext(HttpContext).ExcluirConsulta(dados.Id.Value);
                return Redirect("/consultas");
            }

            if (!ModelState.IsValid)
            {
                IEnumerable<MedicoDto> medicos = await _medVollApiService.WithHttpContext(HttpContext).ListarMedicos(1);
                ViewData["Medicos"] = medicos.ToList();
                return View(PaginaCadastro, dados);
            }

            try
            {
                await _medVollApiService.WithHttpContext(HttpContext).CadastrarConsulta(dados);
                return Redirect("/consultas");
            }
            catch (RegraDeNegocioException ex)
            {
                ViewBag.Erro = ex.Message;
                ViewBag.Dados = dados;
                return View(PaginaCadastro);
            }
        }
    }
}
