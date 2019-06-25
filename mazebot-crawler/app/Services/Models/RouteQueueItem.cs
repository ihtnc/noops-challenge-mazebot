namespace MazebotCrawler.Services.Models
{
    public class RouteQueueItem
    {
        public Coordinates Start { get; set; }
        public string StepsTaken { get; set;}
        public Direction NextStep { get; set; }
    }
}