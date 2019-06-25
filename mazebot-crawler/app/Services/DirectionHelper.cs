using System;
using MazebotCrawler.Services.Models;

namespace MazebotCrawler.Services
{
    public class DirectionHelper
    {
        /// <summary>
        /// Determines the preferred order of Directions to follow, from the start Coordinates to the destination Coordinates.
        /// </summary>
        public static Direction[] GetPreferences(Coordinates start, Coordinates destination)
        {
            // Eules:
            // Prefer to go towards the direction of the destination Coordinates.
            // Prefer to close the gap of the component with the highest distance first (i.e: if the vertical distance is higher than the horizontal distance, prefer a vertical move rather than a horizontal one).
            // Prefer to close the horizontal gap first if the vertical and horizontal distance are the same.
            // Prefer to go east if the start and destination Coordinates fall on the same column.
            // Prefer to go north if the start and destination Coordinates fall on the same row. 

            var preference = new Direction[4];
            var preferNorth = false;
            var preferEast = false;
            
            if (start.Y >= destination.Y) { preferNorth = true; }
            if (start.X <= destination.X) { preferEast = true; }

            var distanceX = Math.Abs(start.X - destination.X);
            var distanceY = Math.Abs(start.Y - destination.Y);

            if (distanceX >= distanceY)
            {
                preference[0] = preferEast ? Direction.East : Direction.West;
                preference[3] = preferEast ? Direction.West : Direction.East;
                preference[1] = preferNorth ? Direction.North : Direction.South;
                preference[2] = preferNorth ? Direction.South : Direction.North;
            }
            else
            {
                preference[0] = preferNorth ? Direction.North : Direction.South;
                preference[3] = preferNorth ? Direction.South : Direction.North;
                preference[1] = preferEast ? Direction.East : Direction.West;
                preference[2] = preferEast ? Direction.West : Direction.East;
            }

            return preference;
        }
    }
}