using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MazebotCrawler.Repositories.Models;
using MazebotCrawler.Services.Models;

namespace MazebotCrawler.Repositories
{
    public class InMemoryStatusRepository : IMazebotSolverStatusRepository
    {
        private readonly List<MazebotSolverStatus> _status;

        public InMemoryStatusRepository()
        {
            _status = new List<MazebotSolverStatus>();
        }

        public async Task<MazebotSolverStatus> Add(string sessionId, MazebotSolverResponse item)
        {
            var row = new MazebotSolverStatus
            {
                SessionId = sessionId,
                Response = item
            };
            _status.Add(row);
            return await Task.FromResult(row);
        }

        public async Task<IEnumerable<MazebotSolverStatus>> Get(string sessionId)
        {
            var list = _status.Where(r => string.Equals(r.SessionId, sessionId));
            return await Task.FromResult(list);
        }
    }
}