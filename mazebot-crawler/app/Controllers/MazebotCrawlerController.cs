using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MazebotCrawler.Services;
using MazebotCrawler.Services.Models;

namespace MazebotCrawler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MazebotCrawlerController : ControllerBase
    {
        [HttpPost("solve/random")]
        public async Task<ActionResult> SolveRandomMap([FromServices] IMazebotSolver solver, int? minSize = null, int? maxSize = null)
        {
            var response = await solver.SolveRandom(minSize, maxSize);
            return new OkObjectResult(response);
        }

        [HttpPost("race/start")]
        public async Task<ActionResult> StartRace([FromServices] IMazebotSolver solver, [FromBody] MazebotSolverRaceRequest request)
        {
            var response = await solver.JoinRace(request);
            return new OkObjectResult(response);
        }

        [HttpGet("race/result/{sessionId}")]
        public async Task<ActionResult> GetRaceStatus([FromServices] IMazebotSolver solver, string sessionId)
        {
            var response = await solver.GetRaceStatus(sessionId);
            if (response == null) { return new NotFoundResult(); }

            return new OkObjectResult(response);
        }

        [HttpGet("session/{sessionId}/status")]
        public async Task<ActionResult> GetStatusDetail([FromServices] IMazebotSolver solver, string sessionId)
        {
            var response = await solver.GetHistory(sessionId);
            if (response?.Any() != true) { return new NotFoundResult(); }

            return new OkObjectResult(response);
        }

        [HttpGet("session/{sessionId}/summary")]
        public async Task<ActionResult> GetStatusSummary([FromServices] IMazebotSolver solver, string sessionId)
        {
            var response = await solver.GetHistorySummary(sessionId);
            if (response?.Any() != true) { return new NotFoundResult(); }

            return new OkObjectResult(response);
        }

        [HttpGet("session/{sessionId}/maze/{mazeId}")]
        public async Task<ActionResult> GetMapImage([FromServices] IMazebotSolver solver, string sessionId, string mazeId)
        {
            var response = await solver.GetMapImage(sessionId, mazeId, false);
            if (response == null) { return new NotFoundResult(); }

            return new FileContentResult(response.Image, response.ContentType);
        }

        [HttpGet("session/{sessionId}/solution/{mazeId}")]
        public async Task<ActionResult> GetMapSolution([FromServices] IMazebotSolver solver, string sessionId, string mazeId)
        {
            var response = await solver.GetMapImage(sessionId, mazeId, true);
            if (response == null) { return new NotFoundResult(); }

            return new FileContentResult(response.Image, response.ContentType);
        }
    }
}
