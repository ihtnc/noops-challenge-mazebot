using MazebotCrawler.Crawlies;
using MazebotCrawler.Crawlies.Models;
using MazebotCrawler.Services.Models;
using Xunit;
using FluentAssertions;

namespace MazebotCrawler.Tests.Crawlies
{
    public class MazeCrawlerSpawnerTests
    {
        private readonly MazeCrawlerSpawner _spwaner;

        public MazeCrawlerSpawnerTests()
        {
            _spwaner = new MazeCrawlerSpawner();
        }

        [Fact]
        public void Spawn_Should_Set_MazeCrawlerContext_Correctly()
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(1, 0),
                NavigationMap = new Map(new char[1][]
                {
                    new char[] {'X', ' ', ' ', 'X'}
                }),
                NavigationMode = CrawlerNavigationMode.Swarm
            };

            var crawler = (MazeCrawler)_spwaner.Spawn(context);

            crawler.CurrentX.Should().Be(context.Start.X);
            crawler.CurrentY.Should().Be(context.Start.Y);
            crawler.CrawlerMap.FloorPlan.Should().BeEquivalentTo(context.NavigationMap.FloorPlan, o => o.WithStrictOrdering());
            crawler.NavigationMode.Should().Be(context.NavigationMode);
        }
    }
}