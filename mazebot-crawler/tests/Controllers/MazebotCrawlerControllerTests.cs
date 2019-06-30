using Microsoft.AspNetCore.Mvc;
using MazebotCrawler.Controllers;
using MazebotCrawler.Services;
using MazebotCrawler.Services.Models;
using MazebotCrawler.Repositories.Models;
using Xunit;
using FluentAssertions;
using NSubstitute;

namespace MazebotCrawler.Tests.Controllers
{
    public class MazebotCrawlerControllerTests
    {
        private readonly MazebotCrawlerController _controller;

        public MazebotCrawlerControllerTests()
        {
            _controller = new MazebotCrawlerController();
        }

        [Fact]
        public void Class_Should_Include_ApiControllerAttribute()
        {
            var t = _controller.GetType();
            t.Should().BeDecoratedWith<ApiControllerAttribute>();
        }

        [Fact]
        public void Class_Should_Include_RouteAttribute()
        {
            var t = _controller.GetType();
            t.Should().BeDecoratedWith<RouteAttribute>(attr => attr.Template == "api/[controller]");
        }

        [Fact]
        public void SolveRandomMap_Should_Include_HttpPostAttribute()
        {
            var t = _controller.GetType();
            t.GetMethod("SolveRandomMap")
             .Should().BeDecoratedWith<HttpPostAttribute>()
             .Which.Template.Should().Be("solve/random");
        }

        [Fact]
        public async void SolveRandomMap_Should_Call_IMazebotSolver_SolveRandom()
        {
            var service = Substitute.For<IMazebotSolver>();
            service.SolveRandom(Arg.Any<int?>(), Arg.Any<int?>()).Returns(new MazebotSolverResponseSummary());

            await _controller.SolveRandomMap(service, 123, 456);

            await service.Received(1).SolveRandom(123, 456);
        }

        [Fact]
        public async void SolveRandomMap_Should_Return_Correctly()
        {
            var service = Substitute.For<IMazebotSolver>();
            var response = new MazebotSolverResponseSummary
            {
                SessionId = "sessionId"
            };
            service.SolveRandom(Arg.Any<int?>(), Arg.Any<int?>()).Returns(response);

            var actual = await _controller.SolveRandomMap(service);

            actual.Should().BeOfType<OkObjectResult>();
            actual.As<OkObjectResult>().Value.Should().Be(response);
        }

        [Fact]
        public void StartRace_Should_Include_HttpPostAttribute()
        {
            var t = _controller.GetType();
            t.GetMethod("StartRace")
             .Should().BeDecoratedWith<HttpPostAttribute>()
             .Which.Template.Should().Be("race/start");
        }

        [Fact]
        public async void StartRace_Should_Call_IMazebotSolver_JoinRace()
        {
            var service = Substitute.For<IMazebotSolver>();
            service.JoinRace(Arg.Any<MazebotSolverRaceRequest>()).Returns(new MazebotSolverRaceResponse("sessionId", "loginId"));

            var request = new MazebotSolverRaceRequest();
            await _controller.StartRace(service, request);

            await service.Received(1).JoinRace(request);
        }

        [Fact]
        public async void StartRace_Should_Return_Correctly()
        {
            var service = Substitute.For<IMazebotSolver>();
            var response = new MazebotSolverRaceResponse("sessionId", "loginId");
            service.JoinRace(Arg.Any<MazebotSolverRaceRequest>()).Returns(response);

            var actual = await _controller.StartRace(service, new MazebotSolverRaceRequest());

            actual.Should().BeOfType<OkObjectResult>();
            actual.As<OkObjectResult>().Value.Should().Be(response);
        }

        [Fact]
        public void GetRaceStatus_Should_Include_HttpGetAttribute()
        {
            var t = _controller.GetType();
            t.GetMethod("GetRaceStatus")
             .Should().BeDecoratedWith<HttpGetAttribute>()
             .Which.Template.Should().Be("race/result/{sessionId}");
        }

        [Fact]
        public async void GetRaceStatus_Should_Call_IMazebotSolver_GetRaceStatus()
        {
            var service = Substitute.For<IMazebotSolver>();
            service.GetRaceStatus(Arg.Any<string>()).Returns(new MazebotCertificate());

            var sessionId = "sessionId";
            await _controller.GetRaceStatus(service, sessionId);

            await service.Received(1).GetRaceStatus(sessionId);
        }

        [Fact]
        public async void GetRaceStatus_Should_Return_Correctly()
        {
            var service = Substitute.For<IMazebotSolver>();
            var response = new MazebotCertificate();
            service.GetRaceStatus(Arg.Any<string>()).Returns(response);

            var actual = await _controller.GetRaceStatus(service, "sessionId");

            actual.Should().BeOfType<OkObjectResult>();
            actual.As<OkObjectResult>().Value.Should().Be(response);
        }

        [Fact]
        public async void GetRaceStatus_Should_Return_Correctly_For_NonExisting_SessionId()
        {
            var service = Substitute.For<IMazebotSolver>();
            service.GetRaceStatus(Arg.Any<string>()).Returns((MazebotCertificate)null);

            var actual = await _controller.GetRaceStatus(service, "sessionId");

            actual.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void GetStatusDetail_Should_Include_HttpGetAttribute()
        {
            var t = _controller.GetType();
            t.GetMethod("GetStatusDetail")
             .Should().BeDecoratedWith<HttpGetAttribute>()
             .Which.Template.Should().Be("session/{sessionId}/status");
        }

        [Fact]
        public async void GetStatusDetail_Should_Call_IMazebotSolver_GetHistory()
        {
            var service = Substitute.For<IMazebotSolver>();
            service.GetHistory(Arg.Any<string>()).Returns(new MazebotSolverStatus[0]);

            var sessionId = "sessionId";
            await _controller.GetStatusDetail(service, sessionId);

            await service.Received(1).GetHistory(sessionId);
        }

        [Fact]
        public async void GetStatusDetail_Should_Return_Correctly()
        {
            var service = Substitute.For<IMazebotSolver>();
            var response = new [] { new MazebotSolverStatus() };
            service.GetHistory(Arg.Any<string>()).Returns(response);

            var actual = await _controller.GetStatusDetail(service, "sessionId");

            actual.Should().BeOfType<OkObjectResult>();
            actual.As<OkObjectResult>().Value.Should().Be(response);
        }

        [Fact]
        public async void GetStatusDetail_Should_Return_Correctly_For_NonExisting_SessionId()
        {
            var service = Substitute.For<IMazebotSolver>();
            var response = new MazebotSolverResponseSummary[0];
            service.GetHistorySummary(Arg.Any<string>()).Returns(response);

            var actual = await _controller.GetStatusSummary(service, "sessionId");

            actual.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void GetStatusSummary_Should_Include_HttpGetAttribute()
        {
            var t = _controller.GetType();
            t.GetMethod("GetStatusSummary")
             .Should().BeDecoratedWith<HttpGetAttribute>()
             .Which.Template.Should().Be("session/{sessionId}/summary");
        }

        [Fact]
        public async void GetStatusSummary_Should_Call_IMazebotSolver_GetHistory()
        {
            var service = Substitute.For<IMazebotSolver>();
            service.GetHistorySummary(Arg.Any<string>()).Returns(new MazebotSolverResponseSummary[0]);

            var sessionId = "sessionId";
            await _controller.GetStatusSummary(service, sessionId);

            await service.Received(1).GetHistorySummary(sessionId);
        }

        [Fact]
        public async void GetStatusSummary_Should_Return_Correctly()
        {
            var service = Substitute.For<IMazebotSolver>();
            var response = new [] { new MazebotSolverResponseSummary() };
            service.GetHistorySummary(Arg.Any<string>()).Returns(response);

            var actual = await _controller.GetStatusSummary(service, "sessionId");

            actual.Should().BeOfType<OkObjectResult>();
            actual.As<OkObjectResult>().Value.Should().Be(response);
        }

        [Fact]
        public async void GetStatusSummary_Should_Return_Correctly_For_NonExisting_SessionId()
        {
            var service = Substitute.For<IMazebotSolver>();
            var response = new MazebotSolverResponseSummary[0];
            service.GetHistorySummary(Arg.Any<string>()).Returns(response);

            var actual = await _controller.GetStatusSummary(service, "sessionId");

            actual.Should().BeOfType<NotFoundResult>();
        }
    }
}