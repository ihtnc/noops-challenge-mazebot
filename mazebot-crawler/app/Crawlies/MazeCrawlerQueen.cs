using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MazebotCrawler.Crawlies.Models;
using MazebotCrawler.Services;
using MazebotCrawler.Services.Models;

namespace MazebotCrawler.Crawlies
{
    /// <summary>
    /// Interface used by components to trigger the MazeCrawlerQueen's actions.
    /// </summary>
    public interface IMazeCrawlerQueen
    {
        void ScanMap(Coordinates start, Coordinates destination, Map map);
        Task<NavigationDetails> Navigate();
    }

    /// <summary>
    /// Interface used by other crawlies to request a swarm of MazeCrawlers from the MazeCrawlerQueen.
    /// </summary>
    public interface ISwarmCoordinator
    {
        IEnumerable<IMazeCrawler> GetSwarm(IMazeCrawlerState requestor);
        ILogger Logger { get; }
    }

    /// <summary>
    /// Interface used by other crawlies to communicate with the MazeCrawlerQueen.
    /// </summary>
    public interface IMazeCrawlerCoordinator
    {
        ISwarmCoordinator RequestSwarm(IMazeCrawlerState requestor);
        void Debrief(IMazeCrawlerState requestor);
        ILogger Logger { get; }
    }

    public class MazeCrawlerQueen : IMazeCrawlerQueen, IMazeCrawlerCoordinator, ISwarmCoordinator
    {
        private Coordinates _start;
        private Coordinates _destination;
        private Map _map;

        private readonly IMazeCrawlerSpawner _spawner;

        public MazeCrawlerQueen(IMazeCrawlerSpawner spawner, ILogger<MazeCrawlerQueen> logger)
        {
            Id = Guid.NewGuid().ToString();
            Logger = logger;

            _spawner = spawner;
        }

        public void ScanMap(Coordinates start, Coordinates destination, Map map)
        {
            _start = start;
            _destination = destination;
            _map = CopyMap(map);

            Trace($"Scanned map:\n{MapHelper.ConvertToString(map.FloorPlan)}");
        }

        public async Task<NavigationDetails> Navigate()
        {
            var map = MaskMap();
            var context = new MazeCrawlerContext
            {
                Start = _start,
                Destination = _destination,
                NavigationMap = map,
                NavigationMode = CrawlerNavigationMode.Scout,
                Coordinator = this
            };

            var crawler = _spawner.Spawn(context);
            var response = await crawler.Navigate();
            if (response.Arrived) { response.PathTaken = MapHelper.SimplifyPath(response.PathTaken); }
            return response;
        }

        public string Id { get; }
        public ILogger Logger { get; private set; }

        public int StartX { get { return _start.X; } }
        public int StartY { get { return _start.Y; } }
        public int DestinationX { get { return _destination.X; } }
        public int DestinationY { get { return _destination.Y; } }
        public Map Map { get { return CopyMap(); } }

        public ISwarmCoordinator RequestSwarm(IMazeCrawlerState requestor)
        {
            Trace($"Crawler {requestor.Id} requested for a swarm.");
            UpdateMap(requestor.CrawlerMap);
            return this;
        }

        public void Debrief(IMazeCrawlerState requestor)
        {
            Trace($"Updating findings from Crawler {requestor.Id}.");
            UpdateMap(requestor.CrawlerMap);
        }

        public IEnumerable<IMazeCrawler> GetSwarm(IMazeCrawlerState requestor)
        {
            Trace($"Checking possible routes from crawler {requestor.Id} location.");

            // Determines the next possible Direction that can be done from the requestor's current Coordinates.
            // Create a MazeCrawler for each Direction.
            var nextSteps = new List<Direction>();
            if(requestor.CanMove(Direction.North)) { Trace($"Crawler {requestor.Id} can move North."); nextSteps.Add(Direction.North); }
            if(requestor.CanMove(Direction.South)) { Trace($"Crawler {requestor.Id} can move South."); nextSteps.Add(Direction.South); }
            if(requestor.CanMove(Direction.East)) { Trace($"Crawler {requestor.Id} can move East."); nextSteps.Add(Direction.East); }
            if(requestor.CanMove(Direction.West)) { Trace($"Crawler {requestor.Id} can move West."); nextSteps.Add(Direction.West); }

            var newStart = new Coordinates(requestor.CurrentX, requestor.CurrentY);
            var newMap = MaskMap();
            var crawlers = new List<IMazeCrawler>();
            foreach(var direction in nextSteps)
            {
                var context = new MazeCrawlerContext
                {
                    Start = newStart,
                    Destination = _destination,
                    NavigationMap = newMap,
                    Coordinator = this,
                    NavigationMode = requestor.NavigationMode
                };
                var crawler = _spawner.Spawn(context);
                Trace($"Moving crawler {requestor.Id} into position.");
                crawler.Move(direction);
                crawlers.Add(crawler);
            }

            return crawlers;
        }

        private void UpdateMap(Map trackUpdates)
        {
            // Updates the MazeCrawlerQueen's map with the TRACK data from the specified map.
            lock(_map.FloorPlan)
            {
                for (var i = 0; i < _map.FloorPlan.Length; i++)
                {
                    for (var j = 0; j < _map.FloorPlan[i].Length; j++)
                    {
                        // Only update the map with TRACK data.
                        if (trackUpdates.FloorPlan[i][j] == Map.MOVEN || trackUpdates.FloorPlan[i][j] == Map.MOVES || trackUpdates.FloorPlan[i][j] == Map.MOVEE || trackUpdates.FloorPlan[i][j] == Map.MOVEW) {
                            _map.FloorPlan[i][j] = trackUpdates.FloorPlan[i][j];
                        }
                    }
                }
            }
        }

        private Map MaskMap()
        {
            // Creates a new map based on the MazeCrawlerQueen's map.
            // Removes the START and DESTN data in this new map.
            // This is because the crawlies receiving this new map only cares about EMPTY, OCCPD, and TRACK data.
            // Update all TRACK data on this new map into OCCUPD.
            // This is so the crawlies receiving this new map will not get confused with the existing TRACK data on the MazeCrawlerQueen's map.
            lock(_map.FloorPlan)
            {
                var masked = new char[_map.FloorPlan.Length][];
                for (var i = 0; i < _map.FloorPlan.Length; i++)
                {
                    masked[i] = new char[_map.FloorPlan[i].Length];
                    for (var j = 0; j < _map.FloorPlan[i].Length; j++)
                    {
                        var isEmpty = _map.FloorPlan[i][j] == Map.EMPTY;
                        var isStart = _map.FloorPlan[i][j] == Map.START;
                        var isDestn = _map.FloorPlan[i][j] == Map.DESTN;
                        var isCrawler = _map.FloorPlan[i][j] == Map.CRWLR;
                        masked[i][j] = isEmpty || isStart || isDestn || isCrawler ? Map.EMPTY : Map.OCCPD;
                    }
                }

                return new Map(masked);
            }
        }

        private Map CopyMap()
        {
            return CopyMap(_map);
        }

        private Map CopyMap(Map source)
        {
            var copy = new char[source.FloorPlan.Length][];
            for (var i = 0; i < source.FloorPlan.Length; i++)
            {
                copy[i] = new char[source.FloorPlan[i].Length];
                for (var j = 0; j < source.FloorPlan[i].Length; j++)
                {
                    copy[i][j] = source.FloorPlan[i][j];
                }
            }
            return new Map(copy);
        }

        private void Trace(string message)
        {
            var queen = $"Queen={Id}";
            Logger.LogTrace($"{queen}:{message}");
        }
    }
}