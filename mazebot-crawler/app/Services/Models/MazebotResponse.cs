namespace MazebotCrawler.Services.Models
{
    public class MazebotResponse
    {
        public string Name { get; set; }
        public string MazePath { get; set; }
        public int[] StartingPosition { get; set; }
        public int[] EndingPosition { get; set; }
        public char[][] Map { get; set; }
    }
}