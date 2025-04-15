using MedVoll.Web.Dtos;

namespace MedVoll.Web.Services
{
    public class MedVollApiService : BaseHttpService, IMedVollApiService
    {
        class ApiUris
        {
            public static string ListarConsultas = "/api/Consulta/listar";
            public static string ObterFormularioConsulta = "/api/Consulta/formulario";
            public static string SalvarConsulta = "/api/Consulta/Salvar";
            public static string ListarMedicos = "/api/Medico/Listar";
            public static string ObterFormularioMedico = "/api/Medico/formulario";
            public static string SalvarMedico = "/api/Medico";
            public static string ListarMedicoPorEspecialidade = "/api/Medico/especialidade";
        }

        private readonly HttpClient _apiClient;
        //private readonly string _carrinhoUrl;
        private readonly ILogger<MedVollApiService> _logger;

        public MedVollApiService(
            IConfiguration configuration
            , HttpClient httpClient
            , ISessionHelper sessionHelper
            , ILogger<MedVollApiService> logger)
            : base(configuration, httpClient, sessionHelper)
        {
            _apiClient = httpClient;
            _logger = logger;
            _baseUri = _configuration["MedVoll.WebApi.Url"];
        }

        public IMedVollApiService WithHttpContext(HttpContext context)
        {
            _httpContext = context;
            return this;
        }

        public async Task<PaginatedList<ConsultaDto>> ListarConsultas(int? page)
        {
            var uri = $"{ApiUris.ListarConsultas}/?page={page}";
            return await GetAuthenticatedAsync<PaginatedList<ConsultaDto>>(uri);
        }

        public async Task<IEnumerable<ConsultaDto>> ObterFormularioConsulta(long? id)
        {
            var uri = $"{ApiUris.ObterFormularioConsulta}/{id}";
            return await GetAuthenticatedAsync<IEnumerable<ConsultaDto>>(uri);
        }

        public async Task<PaginatedList<MedicoDto>> ListarMedicos(int? page)
        {
            var uri = $"{ApiUris.ListarMedicos}/?page={page}";
            return await GetAuthenticatedAsync<PaginatedList<MedicoDto>>(uri);
        }

        //public async Task<UpdateQuantidadeOutput> UpdateItem(string clienteId, UpdateQuantidadeInput input)
        //{
        //    var uri = $"{CarrinhoUris.UpdateItem}/{clienteId}";
        //    return await PutAsync<UpdateQuantidadeOutput>(uri, input);
        //}

        //public async Task<CarrinhoCliente> DefinirQuantidades(ApplicationUser applicationUser, Dictionary<string, int> quantidades)
        //{
        //    var uri = UrlAPIs.Carrinho.UpdateItemCarrinho(_carrinhoUrl);

        //    var atualizarCarrinho = new
        //    {
        //        ClienteId = applicationUser.Id,
        //        Atualizacao = quantidades.Select(kvp => new
        //        {
        //            ItemCarrinhoId = kvp.Key,
        //            NovaQuantidade = kvp.Value
        //        }).ToArray()
        //    };

        //    var conteudoCarrinho = new StringContent(JsonConvert.SerializeObject(atualizarCarrinho), System.Text.Encoding.UTF8, "application/json");

        //    var response = await _apiClient.PutAsync(uri, conteudoCarrinho);

        //    response.EnsureSuccessStatusCode();

        //    var jsonResponse = await response.Content.ReadAsStringAsync();

        //    return JsonConvert.DeserializeObject<CarrinhoCliente>(jsonResponse);
        //}

        //public Task AtualizarCarrinho(CarrinhoCliente carrinhoCliente)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public async Task<bool> Checkout(string clienteId, CadastroViewModel viewModel)
        //{
        //    var uri = $"{CarrinhoUris.Finalizar}/{clienteId}";
        //    return await PostAsync<bool>(uri, viewModel);
        //}

        public override string Scope => "MedVoll.WebAPI";
    }
}
