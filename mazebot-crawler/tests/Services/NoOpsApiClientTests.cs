using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MazebotCrawler.Services;
using MazebotCrawler.Services.Models;
using Xunit;
using FluentAssertions;
using FluentAssertions.Json;
using NSubstitute;

namespace MazebotCrawler.Tests.Services
{
    public class NoOpsApiClientTests
    {
        private readonly string _noopsUrl;
        private readonly IApiRequestProvider _requestProvider;
        private readonly IApiClient _apiClient;

        private readonly INoOpsApiClient _client;

        public NoOpsApiClientTests()
        {
            _noopsUrl = "noopsUrl";
            var options = Substitute.For<IOptionsSnapshot<NoOpsChallengeOptions>>();
            options.Value.Returns(new NoOpsChallengeOptions { NoOpsChallengeApiUrl = _noopsUrl });

            _requestProvider = Substitute.For<IApiRequestProvider>();

            _apiClient = Substitute.For<IApiClient>();

            _client = new NoOpsApiClient(options, _requestProvider, _apiClient, Substitute.For<ILogger<NoOpsApiClient>>());
        }

        [Fact]
        public async void GetMazebotRandom_Should_Call_IApiRequestProvider_CreateGetRequest()
        {
            Dictionary<string, string> queries = null;
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Do<Dictionary<string, string>>(a => queries = a));

            await _client.GetMazebotRandom(null, null);

            _requestProvider.Received(1).CreateGetRequest($"{_noopsUrl}/mazebot/random", Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>());
            queries.Should().BeEmpty();
        }

        [Fact]
        public async void GetMazebotRandom_Should_Add_MinSize_Query_If_Supplied()
        {
            Dictionary<string, string> queries = null;
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Do<Dictionary<string, string>>(a => queries = a));

            var minSize = 123;
            await _client.GetMazebotRandom(minSize, null);

            queries.Should().HaveCount(1);
            queries.Keys.Should().Contain("minSize");
            queries["minSize"].Should().Be(minSize.ToString());
        }

        [Fact]
        public async void GetMazebotRandom_Should_Add_MaxSize_Query_If_Supplied()
        {
            Dictionary<string, string> queries = null;
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Do<Dictionary<string, string>>(a => queries = a));

            var maxSize = 456;
            await _client.GetMazebotRandom(null, maxSize);

            queries.Should().HaveCount(1);
            queries.Keys.Should().Contain("maxSize");
            queries["maxSize"].Should().Be(maxSize.ToString());
        }

        [Fact]
        public async void GetMazebotRandom_Should_Call_IApiClient_SendAsync()
        {
            var request = new HttpRequestMessage();
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>()).Returns(request);

            Func<HttpResponseMessage, Task<MazebotResponse>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotResponse>>>(a => responseMapper = a));

            await _client.GetMazebotRandom(null, null);

            await _apiClient.Received(1).SendAsync(request, Arg.Any<Func<HttpResponseMessage, Task<MazebotResponse>>>());
            responseMapper.Should().NotBeNull();

            request.Dispose();
        }

        [Fact]
        public async void GetMazebotRandom_Should_Check_For_Successful_SendAsync_Response()
        {
            Func<HttpResponseMessage, Task<MazebotResponse>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotResponse>>>(a => responseMapper = a));
            await _client.GetMazebotRandom(null, null);
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            Func<Task> action = async () => await responseMapper(response);

            action.Should().Throw<HttpRequestException>();

            response.Dispose();
        }

        [Fact]
        public async void GetMazebotRandom_Should_Deserialize_SendAsync_Response()
        {
            Func<HttpResponseMessage, Task<MazebotResponse>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotResponse>>>(a => responseMapper = a));
            await _client.GetMazebotRandom(null, null);

            var content = new MazebotResponse
            {
                Name = "name",
                StartingPosition = new [] {1, 2},
                EndingPosition = new [] {3, 4},
                MazePath = "mazePath",
                Map = new char[][] { new char[] {Map.OCCPD, Map.EMPTY, Map.START, Map.DESTN} }
            };

            var stringContent = JsonConvert.SerializeObject(content);
            var response = new HttpResponseMessage
            {
                Content = new StringContent(stringContent)
            };

            var actual = await responseMapper(response);

            actual.Should().BeEquivalentTo(content);
        }

        [Fact]
        public async void GetMazebotRandom_Should_Return_Correctly()
        {
            var response = new MazebotResponse();
            _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<Func<HttpResponseMessage, Task<MazebotResponse>>>()).Returns(response);

            var actual = await _client.GetMazebotRandom(null, null);

            actual.Should().Be(response);
        }

        [Fact]
        public async void GetMazebotRandom_Should_Handle_Exceptions()
        {
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>()).Returns(x => throw new Exception());

            var actual = await _client.GetMazebotRandom(null, null);

            actual.Should().BeNull();
        }

        [Fact]
        public async void RaceLogin_Should_Call_IApiRequestProvider_CreatePostRequest()
        {
            _requestProvider.CreatePostRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<MazebotLoginRequest>(), Arg.Any<Dictionary<string, string>>());

            var request = new MazebotLoginRequest();
            await _client.RaceLogin(request);

            _requestProvider.Received(1).CreatePostRequest($"{_noopsUrl}/mazebot/race/start", Arg.Any<Dictionary<string, string>>(), request, Arg.Any<Dictionary<string, string>>());
        }

        [Fact]
        public async void RaceLogin_Should_Call_IApiClient_SendAsync()
        {
            var request = new HttpRequestMessage();
            _requestProvider.CreatePostRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<MazebotLoginRequest>(), Arg.Any<Dictionary<string, string>>()).Returns(request);

            Func<HttpResponseMessage, Task<MazebotLoginResponse>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotLoginResponse>>>(a => responseMapper = a));

            await _client.RaceLogin(new MazebotLoginRequest());

            await _apiClient.Received(1).SendAsync(request, Arg.Any<Func<HttpResponseMessage, Task<MazebotLoginResponse>>>());
            responseMapper.Should().NotBeNull();

            request.Dispose();
        }

        [Fact]
        public async void RaceLogin_Should_Check_For_Successful_SendAsync_Response()
        {
            Func<HttpResponseMessage, Task<MazebotLoginResponse>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotLoginResponse>>>(a => responseMapper = a));
            await _client.RaceLogin(new MazebotLoginRequest());
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            Func<Task> action = async () => await responseMapper(response);

            action.Should().Throw<HttpRequestException>();

            response.Dispose();
        }

        [Fact]
        public async void RaceLogin_Should_Deserialize_SendAsync_Response()
        {
            Func<HttpResponseMessage, Task<MazebotLoginResponse>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotLoginResponse>>>(a => responseMapper = a));
            await _client.RaceLogin(new MazebotLoginRequest());

            var content = new MazebotLoginResponse
            {
                Message = "message",
                NextMaze = "next"
            };

            var stringContent = JsonConvert.SerializeObject(content);
            var response = new HttpResponseMessage
            {
                Content = new StringContent(stringContent)
            };

            var actual = await responseMapper(response);

            actual.Should().BeEquivalentTo(content);
        }

        [Fact]
        public async void RaceLogin_Should_Return_Correctly()
        {
            var response = new MazebotLoginResponse();
            _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<Func<HttpResponseMessage, Task<MazebotLoginResponse>>>()).Returns(response);

            var actual = await _client.RaceLogin(new MazebotLoginRequest());

            actual.Should().Be(response);
        }

        [Fact]
        public async void RaceLogin_Should_Handle_Exceptions()
        {
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>()).Returns(x => throw new Exception());

            var actual = await _client.RaceLogin(new MazebotLoginRequest());

            actual.Should().BeNull();
        }

        [Fact]
        public async void GetMazebotRaceTrack_Should_Call_IApiRequestProvider_CreateGetRequest()
        {
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>());

            var url = "mapUrl";
            await _client.GetMazebotRaceTrack(url);

            _requestProvider.Received(1).CreateGetRequest($"{_noopsUrl}/{url}", Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>());
        }

        [Fact]
        public async void GetMazebotRaceTrack_Should_Call_IApiClient_SendAsync()
        {
            var request = new HttpRequestMessage();
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>()).Returns(request);

            Func<HttpResponseMessage, Task<MazebotResponse>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotResponse>>>(a => responseMapper = a));

            await _client.GetMazebotRaceTrack("mapUrl");

            await _apiClient.Received(1).SendAsync(request, Arg.Any<Func<HttpResponseMessage, Task<MazebotResponse>>>());
            responseMapper.Should().NotBeNull();

            request.Dispose();
        }

        [Fact]
        public async void GetMazebotRaceTrack_Should_Check_For_Successful_SendAsync_Response()
        {
            Func<HttpResponseMessage, Task<MazebotResponse>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotResponse>>>(a => responseMapper = a));
            await _client.GetMazebotRaceTrack("mapUrl");
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            Func<Task> action = async () => await responseMapper(response);

            action.Should().Throw<HttpRequestException>();

            response.Dispose();
        }

        [Fact]
        public async void GetMazebotRaceTrack_Should_Deserialize_SendAsync_Response()
        {
            Func<HttpResponseMessage, Task<MazebotResponse>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotResponse>>>(a => responseMapper = a));
            await _client.GetMazebotRaceTrack("mapUrl");

            var content = new MazebotResponse
            {
                Name = "name",
                StartingPosition = new [] {1, 2},
                EndingPosition = new [] {3, 4},
                MazePath = "mazePath",
                Map = new char[][] { new char[] {Map.OCCPD, Map.EMPTY, Map.START, Map.DESTN} }
            };

            var stringContent = JsonConvert.SerializeObject(content);
            var response = new HttpResponseMessage
            {
                Content = new StringContent(stringContent)
            };

            var actual = await responseMapper(response);

            actual.Should().BeEquivalentTo(content);
        }

        [Fact]
        public async void GetMazebotRaceTrack_Should_Return_Correctly()
        {
            var response = new MazebotResponse();
            _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<Func<HttpResponseMessage, Task<MazebotResponse>>>()).Returns(response);

            var actual = await _client.GetMazebotRaceTrack("mapUrl");

            actual.Should().Be(response);
        }

        [Fact]
        public async void GetMazebotRaceTrack_Should_Handle_Exceptions()
        {
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>()).Returns(x => throw new Exception());

            var actual = await _client.GetMazebotRaceTrack("mapUrl");

            actual.Should().BeNull();
        }

        [Fact]
        public async void GetMazebotRaceCertificate_Should_Call_IApiRequestProvider_CreateGetRequest()
        {
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>());

            var url = "mapUrl";
            await _client.GetMazebotRaceCertificate(url);

            _requestProvider.Received(1).CreateGetRequest($"{_noopsUrl}/{url}", Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>());
        }

        [Fact]
        public async void GetMazebotRaceCertificate_Should_Call_IApiClient_SendAsync()
        {
            var request = new HttpRequestMessage();
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>()).Returns(request);

            Func<HttpResponseMessage, Task<MazebotCertificate>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotCertificate>>>(a => responseMapper = a));

            await _client.GetMazebotRaceCertificate("mapUrl");

            await _apiClient.Received(1).SendAsync(request, Arg.Any<Func<HttpResponseMessage, Task<MazebotCertificate>>>());
            responseMapper.Should().NotBeNull();

            request.Dispose();
        }

        [Fact]
        public async void GetMazebotRaceCertificate_Should_Check_For_Successful_SendAsync_Response()
        {
            Func<HttpResponseMessage, Task<MazebotCertificate>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotCertificate>>>(a => responseMapper = a));
            await _client.GetMazebotRaceCertificate("mapUrl");
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            Func<Task> action = async () => await responseMapper(response);

            action.Should().Throw<HttpRequestException>();

            response.Dispose();
        }

        [Fact]
        public async void GetMazebotRaceCertificate_Should_Deserialize_SendAsync_Response()
        {
            Func<HttpResponseMessage, Task<MazebotCertificate>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotCertificate>>>(a => responseMapper = a));
            await _client.GetMazebotRaceCertificate("mapUrl");

            var content = new MazebotCertificate
            {
                Completed = DateTimeOffset.Now,
                Elapsed = 123,
                Message = "message"
            };

            var stringContent = JsonConvert.SerializeObject(content);
            var response = new HttpResponseMessage
            {
                Content = new StringContent(stringContent)
            };

            var actual = await responseMapper(response);

            actual.Should().BeEquivalentTo(content);
        }

        [Fact]
        public async void GetMazebotRaceCertificate_Should_Return_Correctly()
        {
            var response = new MazebotCertificate();
            _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<Func<HttpResponseMessage, Task<MazebotCertificate>>>()).Returns(response);

            var actual = await _client.GetMazebotRaceCertificate("mapUrl");

            actual.Should().Be(response);
        }

        [Fact]
        public async void GetMazebotRaceCertificate_Should_Handle_Exceptions()
        {
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>()).Returns(x => throw new Exception());

            var actual = await _client.GetMazebotRaceCertificate("mapUrl");

            actual.Should().BeNull();
        }

        [Fact]
        public async void SolveMazebotMaze_Should_Call_IApiRequestProvider_CreatePostRequest()
        {
            _requestProvider.CreatePostRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<object>(), Arg.Any<Dictionary<string, string>>());

            var url = "mapUrl";
            await _client.SolveMazebotMaze(url, "solution");

            _requestProvider.Received(1).CreatePostRequest($"{_noopsUrl}/{url}", Arg.Any<Dictionary<string, string>>(), Arg.Any<object>(), Arg.Any<Dictionary<string, string>>());
        }

        [Fact]
        public async void SolveMazebotMaze_Should_Call_IApiClient_SendAsync()
        {
            var request = new HttpRequestMessage();
            _requestProvider.CreatePostRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<object>(), Arg.Any<Dictionary<string, string>>()).Returns(request);

            Func<HttpResponseMessage, Task<MazebotResult>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotResult>>>(a => responseMapper = a));

            await _client.SolveMazebotMaze("mapUrl", "solution");

            await _apiClient.Received(1).SendAsync(request, Arg.Any<Func<HttpResponseMessage, Task<MazebotResult>>>());
            responseMapper.Should().NotBeNull();

            request.Dispose();
        }

        [Fact]
        public async void SolveMazebotMaze_Should_Deserialize_SendAsync_Response()
        {
            Func<HttpResponseMessage, Task<MazebotResult>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<MazebotResult>>>(a => responseMapper = a));
            await _client.SolveMazebotMaze("mapUrl", "solution");

            var content = new MazebotResult
            {
                Certificate = "certificate",
                Elapsed = 123,
                Message = "message",
                NextMaze = "next",
                Result = "result",
                ShortestSolutionLength = 456,
                YourSolutionLength = 789
            };

            var stringContent = JsonConvert.SerializeObject(content);
            var response = new HttpResponseMessage
            {
                Content = new StringContent(stringContent)
            };

            var actual = await responseMapper(response);

            actual.Should().BeEquivalentTo(content);
        }

        [Fact]
        public async void SolveMazebotMaze_Should_Return_Correctly()
        {
            var response = new MazebotResult();
            _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<Func<HttpResponseMessage, Task<MazebotResult>>>()).Returns(response);

            var actual = await _client.SolveMazebotMaze("mapUrl", "solution");

            actual.Should().Be(response);
        }

        [Fact]
        public async void SolveMazebotMaze_Should_Handle_Exceptions()
        {
            _requestProvider.CreatePostRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<object>(), Arg.Any<Dictionary<string, string>>()).Returns(x => throw new Exception());

            var actual = await _client.SolveMazebotMaze("mapUrl", "solution");

            actual.Should().BeNull();
        }
    }
}