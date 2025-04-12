
namespace MedVoll.Web.Services
{
    public interface IApiClient
    {
        Task<T?> GetAsync<T>(string endpoint, string token);
        Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, string token);
    }
}
