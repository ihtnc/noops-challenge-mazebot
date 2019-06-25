using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace MazebotCrawler.Services
{
    public interface IApiClient
    {
        Task<T> SendAsync<T>(HttpRequestMessage request, Func<HttpResponseMessage, Task<T>> responseMapper);
        T Send<T>(HttpRequestMessage request, Func<HttpResponseMessage, T> responseMapper);
    }

    public class ApiClient : IApiClient
    {
        private readonly IHttpClientFactory _clientFactory;

        public ApiClient(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<T> SendAsync<T>(HttpRequestMessage request, Func<HttpResponseMessage, Task<T>> responseMapper)
        {
            using(var client = _clientFactory.CreateClient())
            {
                using(var responseMessage = await client.SendAsync(request))
                {
                    return await responseMapper(responseMessage);
                }
            }
        }

        public T Send<T>(HttpRequestMessage request, Func<HttpResponseMessage, T> responseMapper)
        {
            using(var client = _clientFactory.CreateClient())
            {
                var sendTask = client.SendAsync(request);
                sendTask.Wait();

                using(var responseMessage = sendTask.Result)
                {
                    return responseMapper(responseMessage);
                }
            }
        }
    }
}