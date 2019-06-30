using System.Collections.Generic;
using System.Threading.Tasks;
using MazebotCrawler.Repositories.Models;
using MazebotCrawler.Services.Models;

namespace MazebotCrawler.Repositories
{
    public interface IMazebotSolverStatusRepository
    {
        Task<MazebotSolverStatus> Add(string sessionId, MazebotSolverResponse item);
        Task<IEnumerable<MazebotSolverStatus>> Get(string sessionId);
    }
}