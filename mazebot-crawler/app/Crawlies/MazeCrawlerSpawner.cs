using MazebotCrawler.Crawlies.Models;

namespace MazebotCrawler.Crawlies
{
    public interface IMazeCrawlerSpawner
    {
        IMazeCrawler Spawn(MazeCrawlerContext context);
    }

    public class MazeCrawlerSpawner : IMazeCrawlerSpawner
    {
        public IMazeCrawler Spawn(MazeCrawlerContext context)
        {
            return new MazeCrawler(context);
        }
    }
}