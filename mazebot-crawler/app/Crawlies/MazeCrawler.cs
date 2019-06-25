using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        /// <summary>
        /// Calculates the path it would take for the MazeCrawler starting from its current Coordinates in the Map towards the specified destination Coordinates.
        /// </summary>
        Task<NavigationDetails> Navigate(CancellationToken cancelationToken);
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

        public async Task<NavigationDetails> Navigate(CancellationToken cancelationToken)
        {
            NavigationDetails response = null;

            switch(NavigationMode)
            {
                case CrawlerNavigationMode.Scout:
                    // Scout will force the MazeCrawler to traverse by itself the map starting from its current Coordinates until it reaches the destination Coordinates (or a deadend is reached).
                    response = await Task.Run(Scout, cancelationToken);
                    break;
                case CrawlerNavigationMode.Swarm:
                    // Swarm will make the MazeCrawler to traverse the map by itself until it reaches a path that forks.
                    // When this happens, the MazeCrawler will request for other MazeCrawlers and will force each of them to traverse the succeeding path (one for each fork on the path).
                    response = await Swarm(cancelationToken);
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
            if (CurrentX == _destination.X && CurrentY == _destination.Y) { return null; }

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

        public bool CanMove(Direction direction) { return MapHelper.CanMove(CrawlerMap, _current, direction); }

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
            CrawlerMap.FloorPlan[_current.Y][_current.X] = Map.TRACK;
            _stepsTaken.Enqueue((char)Direction.North);
            _current.Y--;
            Recalibrate();
            return this;
        }
        public MazeCrawler MoveSouth(bool checkLegality = true)
        {
            if (checkLegality && !CanMove(Direction.South)) { return this; }
            CrawlerMap.FloorPlan[_current.Y][_current.X] = Map.TRACK;
            _stepsTaken.Enqueue((char)Direction.South);
            _current.Y++;
            Recalibrate();
            return this;
        }
        public MazeCrawler MoveEast(bool checkLegality = true)
        {
            if (checkLegality && !CanMove(Direction.East)) { return this; }
            CrawlerMap.FloorPlan[_current.Y][_current.X] = Map.TRACK;
            _stepsTaken.Enqueue((char)Direction.East);
            _current.X++;
            Recalibrate();
            return this;
        }
        public MazeCrawler MoveWest(bool checkLegality = true)
        {
            if (checkLegality && !CanMove(Direction.West)) { return this; }
            CrawlerMap.FloorPlan[_current.Y][_current.X] = Map.TRACK;
            _stepsTaken.Enqueue((char)Direction.West);
            _current.X--;
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

        private void Recalibrate()
        {
            // Recalculate preferences when there are more than 1 Direction that can be taken from the current Coordinates.
            var moves = 0;
            if (CanMove(Direction.North)) { moves++; }
            if (CanMove(Direction.South)) { moves++; }
            if (CanMove(Direction.East)) { moves++; }
            if (CanMove(Direction.West)) { moves++; }
            var needsCalibration = moves > 1;
            if (NavigationMode == CrawlerNavigationMode.Scout && needsCalibration) { _preference = DirectionHelper.GetPreferences(_current, _destination); }
        }

        private NavigationDetails Scout()
        {
            // Get the next possible directions that can be taken.
            // Get the path that has been taken so far.
            // Put them in a queue.
            var routes = GetNextRoutes();
            var steps = TraceStepsAndReset();
            var queue = GetRouteQueue(_current, steps, routes);

            while (queue.Count > 0)
            {
                // Get an item from the queue and start navigation from there.
                var item = queue.Dequeue();
                var next = item.NextStep;
                var previousSteps = item.StepsTaken;
                _current = item.Start;

                Move(next);

                var nextRoutes = GetNextRoutes();
                while (nextRoutes?.Any() == true)
                {
                    // Continue the navigation using the first Direction in the array (highest preference).
                    // Add the remaining Directions to the front of the queue.
                    var alternateRoutes = nextRoutes.Skip(1).ToArray();
                    var pathSoFar = $"{previousSteps}{TraceSteps()}";
                    var alternateQueue = GetRouteQueue(_current, pathSoFar, alternateRoutes);
                    queue = QueueHelper.AddQueue(alternateQueue, queue);

                    Move(nextRoutes.First());
                    nextRoutes = GetNextRoutes();
                }

                // At this point, it means the path being traversed either reached the destination Coordinates or a dead end.
                // Exit the function and return the appropriate response if the destination Coordinates has been reached.
                // Otherwise, the next item in the queue will be processed and the navigation will be traversed from there.
                var stepsTaken = TraceStepsAndReset();
                if (nextRoutes == null)
                {
                    var pathTaken = $"{previousSteps}{stepsTaken}";
                    return new NavigationDetails { Arrived = true, PathTaken = pathTaken };
                }
            }

            // At this point, every item in the queue has been processed without reaching the destination Coordinates.
            // Because of this, return the appropriate response.
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

        private async Task<NavigationDetails> Swarm(CancellationToken cancelationToken)
        {
            // Get the next possible directions that can be taken.
            // Continue traversing if there's only one direction.
            // Otherwise, call a swarm of MazeCrawlers to traverse each direction.
            var routes = GetNextRoutes();
            while (routes?.Length == 1 && !cancelationToken.IsCancellationRequested)
            {
                Move(routes[0]);
                routes = GetNextRoutes();
            }
            if(cancelationToken.IsCancellationRequested) { routes = new Direction[0]; }

            var path = TraceStepsAndReset();
            if (routes == null)
            {
                return new NavigationDetails { Arrived = true, PathTaken = path };
            }
            else if (routes.Length == 0)
            {
                return new NavigationDetails { Arrived = false };
            }
            else
            {
                var result = await SwarmNavigate(cancelationToken);
                return new NavigationDetails
                {
                    Arrived = result.Arrived,
                    PathTaken = result.Arrived ? $"{path}{result.PathTaken}" : null
                };
            }
        }

#pragma warning disable CS1998,CS4014
        private async Task<NavigationDetails> SwarmNavigate(CancellationToken cancelationToken)
        {
            var swarmCoordinator = _coordinator.RequestSwarm(this);
            var swarm = swarmCoordinator.GetSwarm(this);

            NavigationDetails result = null;
            var tasks = new Dictionary<int, Task>();
            foreach (var crawler in swarm)
            {
                if(cancelationToken.IsCancellationRequested) { break; }

                // Add the Navigate task of each MazeCrawler to a list and remove it once it completes.
                var task = crawler.Navigate(cancelationToken);
                tasks.Add(task.Id, task);
                task.ContinueWith(completed =>
                {
                    lock(tasks)
                    {
                        tasks.Remove(completed.Id);
                        if (completed.Result.Arrived)
                        {
                            // Remove the other Navigate tasks from the list since the destination Coordinates has been reached already.
                            result = completed.Result;
                            _coordinator.ReportArrival(this);
                            tasks.Clear();
                        }
                    }
                });
            }

            while (tasks.Count > 0 && !cancelationToken.IsCancellationRequested)
            {
                // HACK!!! this loop will only end if either all tasks are completed or one of the task found the destination Coordinates.
            }

            return result ?? new NavigationDetails { Arrived = false };
        }
#pragma warning restore CS1998,CS4014
    }
}