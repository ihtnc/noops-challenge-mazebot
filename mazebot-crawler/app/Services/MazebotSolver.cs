using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MazebotCrawler.Crawlies;
using MazebotCrawler.Repositories;
using MazebotCrawler.Repositories.Models;
using MazebotCrawler.Services.Models;

namespace MazebotCrawler.Services
{
    public interface IMazebotSolver
    {
        Task<MazebotSolverResponseSummary> SolveRandom(int? minSize, int? maxSize);
        Task<MazebotSolverRaceResponse> JoinRace(MazebotSolverRaceRequest request);
        Task<MazebotCertificate> GetRaceStatus(string sessionId);
        Task<IEnumerable<MazebotSolverStatus>> GetHistory(string sessionId);
        Task<IEnumerable<MazebotSolverResponseSummary>> GetHistorySummary(string sessionId);
    }

    public class MazebotSolver : IMazebotSolver
    {
        private readonly INoOpsApiClient _apiClient;
        private readonly IMazeCrawlerQueen _queen;
        private readonly IMazebotSolverStatusRepository _repository;

        public MazebotSolver(INoOpsApiClient apiClient, IMazeCrawlerQueen queen, IMazebotSolverStatusRepository repository)
        {
            _apiClient = apiClient;
            _queen = queen;
            _repository = repository;
        }

        public async Task<MazebotSolverResponseSummary> SolveRandom(int? minSize, int? maxSize)
        {
            var maze = await GetRandomMaze(minSize, maxSize);
            var solution = await SolveMaze(maze);
            var result = await PostSolution(maze, solution);

            var id = Guid.NewGuid().ToString();
            var response = new MazebotSolverResponse(id, maze, solution, result);
            await _repository.Add(id, response);
            return response.CreateSummary();
        }

#pragma warning disable CS4014
        public async Task<MazebotSolverRaceResponse> JoinRace(MazebotSolverRaceRequest request)
        {
            var id = Guid.NewGuid().ToString();
            var start = await RaceLogin(new MazebotLoginRequest { Login = request.Login });

            FinishRace(id, start);

            return new MazebotSolverRaceResponse(id, request.Login);
        }
#pragma warning restore CS4014

        public async Task<MazebotCertificate> GetRaceStatus(string sessionId)
        {
            var response = await _repository.Get(sessionId);
            if (!response.Any()) { return null; }

            var end = response.SingleOrDefault(r => !string.IsNullOrWhiteSpace(r.Response?.Certificate));
            if (end == null) { return new MazebotCertificate { Message = "Race is still in progress..." }; }

            return await _apiClient.GetMazebotRaceCertificate(end.Response?.Certificate);
        }

        public async Task<IEnumerable<MazebotSolverStatus>> GetHistory(string sessionId)
        {
            var list = await _repository.Get(sessionId);
            var response = list.OrderByDescending(v => v.CreatedDate);
            return response;
        }

        public async Task<IEnumerable<MazebotSolverResponseSummary>> GetHistorySummary(string sessionId)
        {
            var list = await GetHistory(sessionId);
            var response = list.Select(i => i.Response.CreateSummary());
            return response;
        }

        private async Task<IEnumerable<MazebotSolverResponseSummary>> FinishRace(string sessionId, MazebotLoginResponse start)
        {
            var responses = new List<MazebotSolverResponseSummary>();
            var nextMaze = start.NextMaze;

            while(!string.IsNullOrWhiteSpace(nextMaze))
            {
                var maze = await GetRaceTrack(nextMaze);
                var solution = await SolveMaze(maze);
                var result = await PostSolution(maze, solution);

                var response = new MazebotSolverResponse(sessionId, maze, solution, result);
                await _repository.Add(sessionId, response);
                responses.Add(response.CreateSummary());

                nextMaze = result?.NextMaze;
            }

            return responses;
        }

        private async Task<MazebotResponse> GetRandomMaze(int? minSize, int? maxSize)
        {
            return await _apiClient.GetMazebotRandom(minSize, maxSize);
        }

        private async Task<MazebotResponse> GetRaceTrack(string url)
        {
            return await _apiClient.GetMazebotRaceTrack(url);
        }

        private async Task<MazebotLoginResponse> RaceLogin(MazebotLoginRequest request)
        {
            return await _apiClient.RaceLogin(request);
        }

        private async Task<NavigationDetails> SolveMaze(MazebotResponse mazebotMaze)
        {
            var startX = mazebotMaze.StartingPosition[0];
            var startY = mazebotMaze.StartingPosition[1];
            var start = new Coordinates(startX, startY);

            var destinationX = mazebotMaze.EndingPosition[0];
            var destinationY = mazebotMaze.EndingPosition[1];
            var destination = new Coordinates(destinationX, destinationY);

            var maze = new Map(mazebotMaze.Map);

            _queen.ScanMap(start, destination, maze);

            return await _queen.Navigate();
        }

        private async Task<MazebotResult> PostSolution(MazebotResponse mazebotMaze, NavigationDetails solution)
        {
            MazebotResult result = null;
            if (solution.Arrived)
            {
                result = await _apiClient.SolveMazebotMaze(mazebotMaze.MazePath, solution.PathTaken);
            }
            return result;
        }
    }
}