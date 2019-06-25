using MazebotCrawler.Services.Models;

namespace MazebotCrawler.Services
{
    public class MapHelper
    {
        public static bool CanMove(Map map, Coordinates start, Direction direction)
        {
            switch(direction)
            {
                case Direction.North:
                    return CanMoveNorth(map, start);
                case Direction.South:
                    return CanMoveSouth(map, start);
                case Direction.East:
                    return CanMoveEast(map, start);
                case Direction.West:
                    return CanMoveWest(map, start);
            }

            return false;
        }
        public static bool CanMoveNorth(Map map, Coordinates start)
        {
            var nextY = start.Y - 1;

            var isValid = nextY >= 0;
            var isAllowed = isValid ? map.FloorPlan[nextY][start.X] == Map.EMPTY : false;

            return isAllowed;
        }
        public static bool CanMoveSouth(Map map, Coordinates start)
        {
            var nextY = start.Y + 1;

            var isValid = nextY < map.FloorPlan.Length;
            var isAllowed = isValid ? map.FloorPlan[nextY][start.X] == Map.EMPTY : false;

            return isAllowed;
        }
        public static bool CanMoveEast(Map map, Coordinates start)
        {
            var nextX = start.X + 1;

            var isValid = nextX < map.FloorPlan[start.Y].Length;
            var isAllowed = isValid ? map.FloorPlan[start.Y][nextX] == Map.EMPTY : false;

            return isAllowed;
        }
        public static bool CanMoveWest(Map map, Coordinates start)
        {
            var nextX = start.X - 1;

            var isValid = nextX >= 0;
            var isAllowed = isValid ? map.FloorPlan[start.Y][nextX] == Map.EMPTY : false;

            return isAllowed;
        }
    }
}