using System;
using MazebotCrawler.Services.Models;

namespace MazebotCrawler.Repositories.Models
{
    public class MazebotSolverStatus
    {
        public MazebotSolverStatus()
        {
            CreatedDate = DateTimeOffset.Now;
        }

        public DateTimeOffset CreatedDate { get; set; }
        public string SessionId { get; set; }
        public MazebotSolverResponse Response { get; set; }
    }
}