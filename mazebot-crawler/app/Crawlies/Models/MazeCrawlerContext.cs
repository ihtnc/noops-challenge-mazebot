using MazebotCrawler.Services.Models;

namespace MazebotCrawler.Crawlies.Models
{
    public class MazeCrawlerContext
    {
        public Coordinates Start {get; set;}
        public Coordinates Destination {get; set;}
        public Map NavigationMap {get; set;}
        public CrawlerNavigationMode NavigationMode {get; set;}
        public IMazeCrawlerCoordinator Coordinator {get; set;}
    }
}