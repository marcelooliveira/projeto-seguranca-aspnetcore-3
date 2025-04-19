using MedVoll.Web.Dtos;
using MedVoll.Web.Models;

namespace MedVoll.Web.Services
{
    public interface IMedVollApiService : IBaseHttpService
    {
        IMedVollApiService WithContext(HttpContext context);
        Task<PaginatedList<ConsultaDto>> ListarConsultas(int? page);
        Task<FormularioConsultaDto> ObterFormularioConsulta(long? consultaId);
        Task ExcluirConsulta(long consultaId);
        Task<ConsultaDto> CadastrarConsulta(ConsultaDto input);

        Task<PaginatedList<MedicoDto>> ListarMedicos(int? page);
        Task<MedicoDto> ObterFormularioMedico(long? medicoId);
        Task ExcluirMedico(long medicoId);
        Task<MedicoDto> CadastrarMedico(MedicoDto input);
        Task<IEnumerable<MedicoDto>> ListarMedicosPorEspecialidade(Especialidade especEnum);
    }
}