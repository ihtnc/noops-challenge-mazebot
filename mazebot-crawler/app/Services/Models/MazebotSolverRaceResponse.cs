using System;

namespace MazebotCrawler.Services.Models
{
    public class MazebotSolverRaceResponse
    {
        public MazebotSolverRaceResponse(string sessionId, string login)
        {
            CreatedDate = DateTimeOffset.Now;
            SessionId = sessionId;

            Message = $"{login} is in the race...";
        }

        public DateTimeOffset CreatedDate { get; set; }
        public string SessionId { get; set; }
        public string Message { get; set; }
    }
}