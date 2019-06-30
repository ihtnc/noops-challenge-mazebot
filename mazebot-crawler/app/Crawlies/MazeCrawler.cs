using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MazebotCrawler.Crawlies.Models;
using MazebotCrawler.Services;
using MazebotCrawler.Services.Models;

namespace MazebotCrawler.Crawlies
{
    /// <summary>
    /// Interface used by other crawlies to trigger the MazeCrawler's actions (usually for CrawlerNavigationMode.Swarm).
    /// </summary>
    public interface IMazeCrawler
    {
        string Id {get;}
        /// <summary>
        /// Calculates the path it would take for the MazeCrawler starting from its current Coordinates in the Map towards the specified destination Coordinates.
        /// </summary>
        Task<NavigationDetails> Navigate();
        IMazeCrawler Move(Direction direction);
    }

    /// <summary>
    /// Interface used by other crawlies to query the state of the MazeCrawler.
    /// </summary>
    public interface IMazeCrawlerState
    {
        string Id {get;}
        int CurrentX {get;}
        int CurrentY {get;}

        CrawlerNavigationMode NavigationMode {get;}

        bool CanMove(Direction direction);

        Map CrawlerMap {get;}

        string TraceSteps();
    }

    public class MazeCrawler : IMazeCrawler, IMazeCrawlerState
    {
        private readonly IMazeCrawlerCoordinator _coordinator;
        private readonly Coordinates _start;
        private readonly Coordinates _destination;

        private Coordinates _current;
        private Queue<char> _stepsTaken;
        private Direction[] _preference;

        public MazeCrawler(MazeCrawlerContext context)
        {
            Id = Guid.NewGuid().ToString();
            CrawlerMap = context.NavigationMap;
            NavigationMode = context.NavigationMode;

            _coordinator = context.Coordinator;
            _start = _current = context.Start;
            _destination = context.Destination;

            _stepsTaken = new Queue<char>();
            _preference = DirectionHelper.GetPreferences(_start, _destination);
        }

        public async Task<NavigationDetails> Navigate()
        {
            Trace($"Starting navigation preference: {string.Join(',', _preference )}.");

            NavigationDetails response = null;

            switch(NavigationMode)
            {
                case CrawlerNavigationMode.Scout:
                    // Scout will force the MazeCrawler to traverse by itself the map starting from its current Coordinates until it reaches the destination Coordinates (or a deadend is reached).
                    response = Scout();
                    break;
                case CrawlerNavigationMode.Swarm:
                    // Swarm will make the MazeCrawler to traverse the map by itself until it reaches a path that forks.
                    // When this happens, the MazeCrawler will request for other MazeCrawlers and will force each of them to traverse the succeeding path (one for each fork on the path).
                    response = await Swarm();
                    break;
            }

            return response;
        }

        /// <summary>
        /// Determines the available routes that can be taken from the current Coordinates.
        /// </summary>
        /// <returns>
        /// Returns an array of Direction that can be taken from the current Coordinates, in a certain order of preference.
        /// Returns null if the current Coordinates is the same as the destination.
        /// Returns an empty array if a dead-end is reached.
        /// </returns>
        public Direction[] GetNextRoutes()
        {
            Trace("Finding next routes based on preferences.");

            if (CurrentX == _destination.X && CurrentY == _destination.Y)
            {
                Trace("At the destination!");
                return null;
            }

            var eligibleMoves = new List<Direction>();
            for (var i = 0; i < _preference.Length; i++)
            {
                var direction = _preference[i];
                if (CanMove(direction)) { eligibleMoves.Add(direction); }
            }

            return eligibleMoves.ToArray();
        }

        public string Id { get; }
        public int CurrentX { get { return _current.X; } }
        public int CurrentY { get { return _current.Y; } }
        public CrawlerNavigationMode NavigationMode { get; }
        public Map CrawlerMap { get; private set; }

        public bool CanMove(Direction direction)
        {
            var able = MapHelper.CanMove(CrawlerMap, _current, direction);
            if (able) { Trace($"Can move {direction}."); }
            return able;
        }

        public IMazeCrawler Move(Direction direction)
        {
            return Move(direction, true);
        }
        public MazeCrawler Move(Direction direction, bool checkLegality = true)
        {
            switch(direction)
            {
                case Direction.North:
                    return MoveNorth(checkLegality);
                case Direction.South:
                    return MoveSouth(checkLegality);
                case Direction.East:
                    return MoveEast(checkLegality);
                case Direction.West:
                    return MoveWest(checkLegality);
            }

            return this;
        }
        public MazeCrawler MoveNorth(bool checkLegality = true)
        {
            if (checkLegality && !CanMove(Direction.North)) { return this; }
            Trace("Moving North.");
            MarkTrack(Map.MOVEN);
            _stepsTaken.Enqueue((char)Direction.North);
            _current.Y--;
            MarkTrack(Map.CRWLR);
            Trace($"Current map:\n{MapHelper.ConvertToString(CrawlerMap.FloorPlan)}");
            Recalibrate();
            return this;
        }
        public MazeCrawler MoveSouth(bool checkLegality = true)
        {
            if (checkLegality && !CanMove(Direction.South)) { return this; }
            Trace("Moving South.");
            MarkTrack(Map.MOVES);
            _stepsTaken.Enqueue((char)Direction.South);
            _current.Y++;
            MarkTrack(Map.CRWLR);
            Trace($"Current map:\n{MapHelper.ConvertToString(CrawlerMap.FloorPlan)}");
            Recalibrate();
            return this;
        }
        public MazeCrawler MoveEast(bool checkLegality = true)
        {
            if (checkLegality && !CanMove(Direction.East)) { return this; }
            Trace("Moving East.");
            MarkTrack(Map.MOVEE);
            _stepsTaken.Enqueue((char)Direction.East);
            _current.X++;
            MarkTrack(Map.CRWLR);
            Trace($"Current map:\n{MapHelper.ConvertToString(CrawlerMap.FloorPlan)}");
            Recalibrate();
            return this;
        }
        public MazeCrawler MoveWest(bool checkLegality = true)
        {
            if (checkLegality && !CanMove(Direction.West)) { return this; }
            Trace("Moving West.");
            MarkTrack(Map.MOVEW);
            _stepsTaken.Enqueue((char)Direction.West);
            _current.X--;
            MarkTrack(Map.CRWLR);
            Trace($"Current map:\n{MapHelper.ConvertToString(CrawlerMap.FloorPlan)}");
            Recalibrate();
            return this;
        }

        public string TraceSteps()
        {
            return string.Concat(_stepsTaken.ToArray());
        }

        private string TraceStepsAndReset()
        {
            var path = new StringBuilder();
            while (_stepsTaken.Count != 0) { path.Append(_stepsTaken.Dequeue()); }
            return path.ToString();
        }

        private void MarkTrack(char mark) { CrawlerMap.FloorPlan[_current.Y][_current.X] = mark; }

        private void Recalibrate()
        {
            // Recalculate preferences when there are more than 1 Direction that can be taken from the current Coordinates.
            var moves = 0;
            if (CanMove(Direction.North)) { moves++; }
            if (CanMove(Direction.South)) { moves++; }
            if (CanMove(Direction.East)) { moves++; }
            if (CanMove(Direction.West)) { moves++; }
            var needsCalibration = moves > 1;
            if (NavigationMode == CrawlerNavigationMode.Scout && needsCalibration)
            {
                _preference = DirectionHelper.GetPreferences(_current, _destination);
                Trace($"Recalibrated preference: {string.Join(',', _preference )}.");
            }
        }

        private NavigationDetails Scout()
        {
            // Get the next possible directions that can be taken.
            // Get the path that has been taken so far.
            // Put them in a queue.
            var routes = GetNextRoutes();
            Trace($"Found {routes.Length} possible route(s).");

            var steps = TraceStepsAndReset();
            Trace($"Steps so far: {steps}.");

            Trace("Adding possible routes in the queue.");
            var queue = GetRouteQueue(_current, steps, routes);

            while (queue.Count > 0)
            {
                // Get an item from the queue and start navigation from there.
                var item = queue.Dequeue();
                var next = item.NextStep;
                var previousSteps = item.StepsTaken;
                Trace($"Next route in queue: {previousSteps} + {next} from ({item.Start.X},{item.Start.Y}).");
                Trace($"Items in queue: {queue.Count()}.");
                MarkTrack(Map.EMPTY);

                _current = item.Start;
                Trace($"Relocating to route origin.");

                Move(next);

                var nextRoutes = GetNextRoutes();
                Trace($"Found {routes.Length} possible route(s).");
                while (nextRoutes?.Any() == true)
                {
                    // Continue the navigation using the first Direction in the array (highest preference).
                    // Add the remaining Directions to the front of the queue.
                    var alternateRoutes = nextRoutes.Skip(1).ToArray();
                    var pathSoFar = $"{previousSteps}{TraceSteps()}";
                    Trace($"Adding {alternateRoutes.Count()} possible route(s) in the queue.");
                    var alternateQueue = GetRouteQueue(_current, pathSoFar, alternateRoutes);
                    queue = QueueHelper.AddQueue(alternateQueue, queue);
                    Trace($"Items in queue: {queue.Count()}.");

                    Move(nextRoutes.First());
                    nextRoutes = GetNextRoutes();
                    Trace($"Found {routes.Length} possible route(s).");
                }

                // At this point, it means the path being traversed either reached the destination Coordinates or a dead end.
                // Exit the function and return the appropriate response if the destination Coordinates has been reached.
                // Otherwise, the next item in the queue will be processed and the navigation will be traversed from there.
                var stepsTaken = TraceStepsAndReset();
                Trace($"Steps so far: {steps}.");
                if (nextRoutes == null)
                {
                    Trace($"Arrived at destination!");
                    var pathTaken = $"{previousSteps}{stepsTaken}";
                    return new NavigationDetails { Arrived = true, PathTaken = pathTaken };
                }
            }

            // At this point, every item in the queue has been processed without reaching the destination Coordinates.
            // Because of this, return the appropriate response.
            Trace($"Arrived at a deadend.");
            return new NavigationDetails { Arrived = false };
        }

        private Queue<RouteQueueItem> GetRouteQueue(Coordinates start, string stepsTaken, Direction[] next)
        {
            var queue = new Queue<RouteQueueItem>();
            for(var i = 0; i < next.Length; i++)
            {
                var item = new RouteQueueItem
                {
                    Start = start,
                    StepsTaken = stepsTaken,
                    NextStep = next[i]
                };
                queue.Enqueue(item);
            }
            return queue;
        }

        private async Task<NavigationDetails> Swarm()
        {
            // Get the next possible directions that can be taken.
            // Continue traversing if there's only one direction.
            // Otherwise, call a swarm of MazeCrawlers to traverse each direction.
            var routes = GetNextRoutes();
            if (routes != null) { Trace($"Found {routes.Length} possible route(s)."); }

            while (routes?.Length == 1)
            {
                Move(routes[0]);

                routes = GetNextRoutes();
                if (routes != null) { Trace($"Found {routes.Length} possible route(s)."); }
            }

            var path = TraceStepsAndReset();
            Trace($"Steps so far: {path}.");
            if (routes == null)
            {
                Trace($"Arrived at destination!");
                _coordinator.Debrief(this);
                return new NavigationDetails { Arrived = true, PathTaken = path };
            }
            else if (routes.Length == 0)
            {
                Trace($"Arrived at a deadend.");
                _coordinator.Debrief(this);
                return new NavigationDetails { Arrived = false };
            }
            else
            {
                var result = await SwarmNavigate();
                _coordinator.Debrief(this);
                return new NavigationDetails
                {
                    Arrived = result.Arrived,
                    PathTaken = result.Arrived ? $"{path}{result.PathTaken}" : null
                };
            }
        }

#pragma warning disable CS1998,CS4014
        private async Task<NavigationDetails> SwarmNavigate()
        {
            var swarmCoordinator = _coordinator.RequestSwarm(this);
            var swarm = swarmCoordinator.GetSwarm(this);
            Trace($"Received {swarm.Count()} crawler(s) from the swarm.");

            NavigationDetails result = null;
            var tasks = new Dictionary<int, Task>();
            var cancelationSource = new CancellationTokenSource();

            foreach (var crawler in swarm)
            {
                // Add the Navigate task of each MazeCrawler to a list and remove it once it completes.
                Trace($"Crawler {crawler.Id} to continue with the navigation.");
                var task = Task.Run(() => crawler.Navigate(), cancelationSource.Token);

                tasks.Add(task.Id, task);
                task.ContinueWith(completed =>
                {
                    lock(tasks)
                    {
                        Trace($"Swarm with task#{completed.Id} completed its navigation.");

                        if (completed.IsCompletedSuccessfully && completed.Result.Arrived)
                        {
                            // Cancel the other Navigate tasks from the list since the destination Coordinates has been reached already.
                            result = completed.Result;
                            Trace($"Crawler with task#{completed.Id} arrived. Reporting arrival.");
                            cancelationSource.Cancel();
                        }

                        tasks.Remove(completed.Id);
                        Trace($"Swarm has {tasks.Count()} task(s) left.");

                    }
                });

                while (tasks.Count() > 0) { await Task.Delay(25); };
            }

            cancelationSource.Dispose();

            return result ?? new NavigationDetails { Arrived = false };
        }
#pragma warning restore CS1998,CS4014

        private void Trace(string message)
        {
            var crawler = $"Crawler={Id}";
            var current = $"({CurrentX},{CurrentY})";
            _coordinator.Logger.LogTrace($"{crawler}{current}:{message}");
        }
    }
}