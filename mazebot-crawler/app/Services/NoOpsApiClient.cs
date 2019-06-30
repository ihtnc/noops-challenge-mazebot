using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MazebotCrawler.Services.Models;

namespace MazebotCrawler.Services
{
    public interface INoOpsApiClient
    {
        Task<MazebotResponse> GetMazebotRandom(int? minSize, int? maxSize);
        Task<MazebotLoginResponse> RaceLogin(MazebotLoginRequest request);
        Task<MazebotResponse> GetMazebotRaceTrack(string mapUrl);
        Task<MazebotCertificate> GetMazebotRaceCertificate(string certificateUrl);
        Task<MazebotResult> SolveMazebotMaze(string mapUrl, string solution);
    }

    public class NoOpsApiClient : INoOpsApiClient
    {
        private readonly string _apiUrl;
        private readonly IApiRequestProvider _requestProvider;
        private readonly IApiClient _client;
        private readonly ILogger _logger;

        public NoOpsApiClient(IOptionsSnapshot<NoOpsChallengeOptions> options, IApiRequestProvider requestProvider, IApiClient client, ILogger<NoOpsApiClient> logger)
        {
            _apiUrl = options?.Value?.NoOpsChallengeApiUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(options));
            _requestProvider = requestProvider;
            _client = client;
            _logger = logger;
        }

        public async Task<MazebotResponse> GetMazebotRandom(int? minSize, int? maxSize)
        {
            var url = $"{_apiUrl}/mazebot/random";

            try
            {
                var queries = new Dictionary<string, string>();
                if (minSize != null) { queries.Add("minSize", minSize.ToString()); }
                if (maxSize != null) { queries.Add("maxSize", maxSize.ToString()); }

                var request = _requestProvider.CreateGetRequest(url, queries: queries);

                var response = await _client.SendAsync(request, async r =>
                {
                    r.EnsureSuccessStatusCode();
                    var content = await r.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeAnonymousType(content, new MazebotResponse());
                });

                return response;
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"An error has occurred while trying to call the mazebot API {url}.");
                return null;
            }
        }

        public async Task<MazebotLoginResponse> RaceLogin(MazebotLoginRequest login)
        {
            var url = $"{_apiUrl}/mazebot/race/start";

            try
            {
                var request = _requestProvider.CreatePostRequest(url, content: login);

                var response = await _client.SendAsync(request, async r =>
                {
                    r.EnsureSuccessStatusCode();
                    var content = await r.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeAnonymousType(content, new MazebotLoginResponse());
                });

                return response;
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"An error has occurred while trying to call the mazebot API {url}.");
                return null;
            }
        }

        public async Task<MazebotResponse> GetMazebotRaceTrack(string mapUrl)
        {
            var url = $"{_apiUrl}/{mapUrl.TrimStart('/')}";

            try
            {
                var request = _requestProvider.CreateGetRequest(url);

                var response = await _client.SendAsync(request, async r =>
                {
                    r.EnsureSuccessStatusCode();
                    var content = await r.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeAnonymousType(content, new MazebotResponse());
                });

                return response;
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"An error has occurred while trying to call the mazebot API {url}.");
                return null;
            }
        }

        public async Task<MazebotCertificate> GetMazebotRaceCertificate(string certificateUrl)
        {
            var url = $"{_apiUrl}/{certificateUrl.TrimStart('/')}";

            try
            {
                var request = _requestProvider.CreateGetRequest(url);

                var response = await _client.SendAsync(request, async r =>
                {
                    r.EnsureSuccessStatusCode();
                    var content = await r.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeAnonymousType(content, new MazebotCertificate());
                });

                return response;
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"An error has occurred while trying to call the mazebot API {url}.");
                return null;
            }
        }

        public async Task<MazebotResult> SolveMazebotMaze(string mapUrl, string solution)
        {
            var url = $"{_apiUrl}/{mapUrl.TrimStart('/')}";

            try
            {
                var message =  new { directions = solution };
                var request = _requestProvider.CreatePostRequest(url, content: message);

                var response = await _client.SendAsync(request, async r =>
                {
                    var content = await r.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeAnonymousType(content, new MazebotResult());
                });

                return response;
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"An error has occurred while trying to call the mazebot API {url}.");
                return null;
            }
        }
    }
}