using MedVoll.Web.Dtos;

namespace MedVoll.Web.Services
{
    public interface IMedVollApiService : IBaseHttpService
    {
        string Scope { get; }
        IMedVollApiService WithHttpContext(HttpContext context);
        Task<PaginatedList<ConsultaDto>> ListarConsultas(int? page);
        Task<IEnumerable<ConsultaDto>> ObterFormularioConsulta(long? id);
        Task<PaginatedList<MedicoDto>> ListarMedicos(int? page);
    }
}