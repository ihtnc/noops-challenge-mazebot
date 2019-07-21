using System;

namespace MazebotCrawler.Services.Models
{
    public class MazebotSolverResponseSummary
    {
        public string SessionId { get; set; }
        public string MazeId { get; set; }
        public string MazePath { get; set; }
        public string Message { get; set; }
        public string NextMaze { get; set; }
        public string Certificate { get; set;}
    }
}