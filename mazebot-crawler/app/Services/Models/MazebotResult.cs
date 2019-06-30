namespace MazebotCrawler.Services.Models
{
    public class MazebotResult
    {
        public string Result { get; set; }
        public string Message { get; set; }
        public decimal Elapsed { get; set; }
        public int ShortestSolutionLength { get; set; }
        public int YourSolutionLength { get; set; }
        public string NextMaze { get; set; }
        public string Certificate { get; set; }
    }
}
