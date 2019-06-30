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

        public static char CRWLR = 'C';
        public static char MOVEN = '^';
        public static char MOVES = 'V';
        public static char MOVEE = '>';
        public static char MOVEW = '<';

        public static char START = 'A';
        public static char DESTN = 'B';

        public char[][] FloorPlan { get; }
    }
}