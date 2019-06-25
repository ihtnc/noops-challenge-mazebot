namespace MazebotCrawler.Services.Models
{
    public struct Map
    {
        public Map(char[][] floorPlan)
        {
            FloorPlan = floorPlan;
        }

        public static char OCCPD = 'X';
        public static char EMPTY = ' ';
        public static char TRACK = 'O';
        public static char START = 'A';
        public static char DESTN = 'B';

        public char[][] FloorPlan { get; }
    }
}