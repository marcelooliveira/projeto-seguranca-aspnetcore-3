using MedVoll.Web.Dtos;
using MedVoll.Web.Models;

namespace MedVoll.Web.Services
{
    public interface IMedVollApiService : IBaseHttpService
    {
        string Scope { get; }
        IMedVollApiService WithHttpContext(HttpContext context);
        Task<PaginatedList<ConsultaDto>> ListarConsultas(int? page);
        Task<FormularioConsultaDto> ObterFormularioConsulta(long? consultaId);
        Task ExcluirConsulta(long consultaId);
        Task<ConsultaDto> CadastrarConsulta(ConsultaDto input);

        Task<PaginatedList<MedicoDto>> ListarMedicos(int? page);
        Task<IEnumerable<MedicoDto>> ListarMedicosPorEspecialidade(Especialidade especEnum);
    }
}