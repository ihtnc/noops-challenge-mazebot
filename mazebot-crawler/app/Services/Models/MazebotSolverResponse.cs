using System;
using Newtonsoft.Json;

namespace MazebotCrawler.Services.Models
{
    public class MazebotSolverResponse
    {
        public MazebotSolverResponse(string sessionId, MazebotResponse maze, NavigationDetails solution, MazebotResult result)
        {
            MazeId = Guid.NewGuid().ToString();
            SessionId = sessionId;
            MazePath = maze?.MazePath;
            DirectionsResult = result?.Result;
            Message = result?.Message;
            Elapsed = result?.Elapsed ?? 0;
            SolutionLength = result != null ? $"{result.YourSolutionLength} (min: {result.ShortestSolutionLength})" : null;
            NextMaze = result?.NextMaze;
            Certificate = result?.Certificate;

            RawMaze = JsonConvert.SerializeObject(maze, Formatting.None);
            RawSolution = JsonConvert.SerializeObject(solution, Formatting.None);
            RawResult = JsonConvert.SerializeObject(result, Formatting.None);

            Maze = maze;
            Solution = solution;
            Result = result;
        }

        public string MazeId { get; set; }
        public string SessionId { get; set; }
        public string MazePath { get; set; }
        public string DirectionsResult { get; set; }
        public string Message { get; set; }
        public decimal Elapsed { get; set; }
        public string SolutionLength { get; set; }
        public string NextMaze { get; set; }
        public string Certificate { get; set; }

        public string RawMaze { get; set; }
        public string RawSolution { get; set; }
        public string RawResult { get; set; }

        [JsonIgnore]
        internal MazebotResponse Maze { get; set; }
        [JsonIgnore]
        internal NavigationDetails Solution { get; set; }
        [JsonIgnore]
        internal MazebotResult Result { get; set; }

        public MazebotSolverResponseSummary CreateSummary()
        {
            return new MazebotSolverResponseSummary
            {
                SessionId = SessionId,
                MazeId = MazeId,
                MazePath = MazePath,
                Message = Message,
                NextMaze = NextMaze,
                Certificate = Certificate
            };
        }
    }
}