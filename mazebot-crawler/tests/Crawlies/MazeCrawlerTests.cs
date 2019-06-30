using System.Linq;
using System.Threading.Tasks;
using MazebotCrawler.Crawlies;
using MazebotCrawler.Crawlies.Models;
using MazebotCrawler.Services.Models;
using Xunit;
using FluentAssertions;
using NSubstitute;

namespace MazebotCrawler.Tests.Crawlies
{
    public class MazebotCrawlerTests
    {
        private readonly Map _canMoveMap;
        private readonly Map _moveMap;

        public MazebotCrawlerTests()
        {
            _canMoveMap = new Map(new char[][]
            {
                new [] {Map.EMPTY, Map.OCCPD, Map.OCCPD, Map.OCCPD, Map.EMPTY},
                new [] {Map.OCCPD, Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.OCCPD},
                new [] {Map.OCCPD, Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.OCCPD},
                new [] {Map.OCCPD, Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.OCCPD},
                new [] {Map.EMPTY, Map.OCCPD, Map.OCCPD, Map.OCCPD, Map.EMPTY}
            });

            _moveMap = new Map(new char[][]
            {
                new [] {Map.OCCPD, Map.OCCPD, Map.OCCPD, Map.OCCPD},
                new [] {Map.OCCPD, Map.EMPTY, Map.EMPTY, Map.OCCPD},
                new [] {Map.OCCPD, Map.EMPTY, Map.EMPTY, Map.OCCPD},
                new [] {Map.OCCPD, Map.OCCPD, Map.OCCPD, Map.OCCPD}
            });
        }

        [Fact]
        public void Constructor_Should_Set_Current_Coordinate()
        {
            var x = 1;
            var y = 3;
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(x, y),
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };

            var crawler = new MazeCrawler(context);

            crawler.CurrentX.Should().Be(x);
            crawler.CurrentY.Should().Be(y);
        }

        [Fact]
        public void Constructor_Should_Set_CrawlerMap()
        {
            var map = new Map(new char[][]
            {
                new [] { Map.OCCPD }
            });
            var context = new MazeCrawlerContext
            {
                NavigationMap = map,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };

            var crawler = new MazeCrawler(context);

            crawler.CrawlerMap.Should().Be(map);
        }

        [Fact]
        public void GetNextRoutes_Should_Return_Null_If_Current_Is_At_Destination()
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(6, 4),
                Destination = new Coordinates(6, 4),
                NavigationMap = new Map(new char[0][]),
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.GetNextRoutes().Should().BeNull();
        }

        [Theory]
        [InlineData(0, 1, 1, 3, Direction.East)]
        [InlineData(2, 0, 0, 1, Direction.South)]
        [InlineData(3, 2, 2, 0, Direction.West)]
        [InlineData(1, 3, 3, 2, Direction.North)]
        public void GetNextRoutes_Should_Return_Directions_Based_On_Availability(int startX, int startY, int destinationX, int destinationY, Direction expected)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(startX, startY),
                Destination = new Coordinates(destinationX, destinationY),
                NavigationMap = new Map(new char[][]
                {
                    new char[] {Map.OCCPD, Map.OCCPD, Map.EMPTY, Map.OCCPD},
                    new char[] {Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.OCCPD},
                    new char[] {Map.OCCPD, Map.EMPTY, Map.EMPTY, Map.EMPTY},
                    new char[] {Map.OCCPD, Map.EMPTY, Map.OCCPD, Map.OCCPD}
                }),
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            var routes = crawler.GetNextRoutes();

            routes.Should().HaveCount(1);
            routes.First().Should().Be(expected);
        }

        [Fact]
        public void GetNextRoutes_Should_Return_Empty_If_There_Are_No_Available_Routes()
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(0, 1),
                Destination = new Coordinates(2, 3),
                NavigationMap = new Map(new char[][]
                {
                    new char[] {Map.OCCPD, Map.OCCPD, Map.EMPTY},
                    new char[] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new char[] {Map.OCCPD, Map.OCCPD, Map.EMPTY},
                    new char[] {Map.EMPTY, Map.EMPTY, Map.EMPTY}
                }),
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            var routes = crawler.GetNextRoutes();

            routes.Should().BeEmpty();
        }

        [Theory]
        [InlineData(0, 0, 1, 2, new [] {Direction.South, Direction.East})]
        [InlineData(0, 1, 2, 1, new [] {Direction.East, Direction.North, Direction.South})]
        [InlineData(1, 2, 1, 0, new [] {Direction.North, Direction.East, Direction.West})]
        [InlineData(1, 1, 2, 2, new [] {Direction.East, Direction.South, Direction.North, Direction.West})]
        [InlineData(2, 2, 0, 1, new [] {Direction.West, Direction.North})]
        public void GetNextRoutes_Should_Return_Directions_Based_On_Preference(int startX, int startY, int destinationX, int destinationY, Direction[] expected)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(startX, startY),
                Destination = new Coordinates(destinationX, destinationY),
                NavigationMap = new Map(new char[][]
                {
                    new char[] {Map.EMPTY, Map.EMPTY, Map.EMPTY},
                    new char[] {Map.EMPTY, Map.EMPTY, Map.EMPTY},
                    new char[] {Map.EMPTY, Map.EMPTY, Map.EMPTY}
                }),
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            var routes = crawler.GetNextRoutes();

            routes.Should().HaveSameCount(expected);
            routes.Should().Equal(expected);
        }

        [Theory]
        [InlineData(CrawlerNavigationMode.Scout, 0, 0, 0, 4, true, "SSSS")]
        [InlineData(CrawlerNavigationMode.Scout, 0, 0, 0, 6, false, null)]
        [InlineData(CrawlerNavigationMode.Scout, 0, 4, 0, 0, true, "NNNN")]
        [InlineData(CrawlerNavigationMode.Scout, 0, 6, 0, 0, false, null)]
        [InlineData(CrawlerNavigationMode.Swarm, 0, 0, 0, 4, true, "SSSS")]
        [InlineData(CrawlerNavigationMode.Swarm, 0, 0, 0, 6, false, null)]
        [InlineData(CrawlerNavigationMode.Swarm, 0, 4, 0, 0, true, "NNNN")]
        [InlineData(CrawlerNavigationMode.Swarm, 0, 6, 0, 0, false, null)]
        public async void Navigate_Should_Handle_Simple_Vertical_Maps(CrawlerNavigationMode mode, int startX, int startY, int destinationX, int destinationY, bool arrived, string path)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(startX, startY),
                Destination = new Coordinates(destinationX, destinationY),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY},
                    new [] {Map.EMPTY},
                    new [] {Map.EMPTY},
                    new [] {Map.EMPTY},
                    new [] {Map.EMPTY},
                    new [] {Map.OCCPD},
                    new [] {Map.EMPTY}
                }),
                NavigationMode = mode,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            var response = await crawler.Navigate();

            response.Arrived.Should().Be(arrived);
            response.PathTaken.Should().Be(path);
        }

        [Theory]
        [InlineData(CrawlerNavigationMode.Scout, 0, 0, 2, 0, true, "EE")]
        [InlineData(CrawlerNavigationMode.Scout, 0, 0, 4, 0, false, null)]
        [InlineData(CrawlerNavigationMode.Scout, 2, 0, 0, 0, true, "WW")]
        [InlineData(CrawlerNavigationMode.Scout, 4, 0, 0, 0, false, null)]
        [InlineData(CrawlerNavigationMode.Swarm, 0, 0, 2, 0, true, "EE")]
        [InlineData(CrawlerNavigationMode.Swarm, 0, 0, 4, 0, false, null)]
        [InlineData(CrawlerNavigationMode.Swarm, 2, 0, 0, 0, true, "WW")]
        [InlineData(CrawlerNavigationMode.Swarm, 4, 0, 0, 0, false, null)]
        public async void Navigate_Should_Handle_Simple_Horizontal_Maps(CrawlerNavigationMode mode, int startX, int startY, int destinationX, int destinationY, bool arrived, string path)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(startX, startY),
                Destination = new Coordinates(destinationX, destinationY),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.OCCPD, Map.EMPTY}
                }),
                NavigationMode = mode,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            var response = await crawler.Navigate();

            response.Arrived.Should().Be(arrived);
            response.PathTaken.Should().Be(path);
        }

        [Theory]
        [InlineData(CrawlerNavigationMode.Scout, 0, 2, 2, 5, true, "SSSSSEEEENNNNNNWWSSSS")]
        [InlineData(CrawlerNavigationMode.Scout, 2, 5, 4, 1, true, "NNNNEE")]
        [InlineData(CrawlerNavigationMode.Scout, 2, 5, 0, 0, false, null)]
        [InlineData(CrawlerNavigationMode.Swarm, 0, 2, 2, 5, true, "SSSSSEEEENNNNNNWWSSSS")]
        [InlineData(CrawlerNavigationMode.Swarm, 2, 5, 4, 1, true, "NNNNEE")]
        [InlineData(CrawlerNavigationMode.Swarm, 2, 5, 0, 0, false, null)]
        public async void Navigate_Should_Handle_Single_Route_Maps(CrawlerNavigationMode mode, int startX, int startY, int destinationX, int destinationY, bool arrived, string path)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(startX, startY),
                Destination = new Coordinates(destinationX, destinationY),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.OCCPD, Map.OCCPD, Map.OCCPD, Map.OCCPD},
                    new [] {Map.OCCPD, Map.OCCPD, Map.EMPTY, Map.EMPTY, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.OCCPD, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.EMPTY}
                }),
                NavigationMode = mode,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            var response = await crawler.Navigate();

            response.Arrived.Should().Be(arrived);
            response.PathTaken.Should().Be(path);
        }

        [Theory]
        [InlineData(0, 0, 1, 7, "SSSSSSSE")]
        [InlineData(1, 7, 1, 0, "ENNNNNNNW")]
        [InlineData(0, 4, 2, 4, "NNNNEESSSS")]
        [InlineData(2, 2, 0, 1, "NNWWS")]
        [InlineData(1, 7, 0, 5, "WNN")]
        [InlineData(2, 3, 2, 7, "SSSS")]
        [InlineData(0, 0, 2, 2, "EESS")]
        public async void Navigate_On_Scout_Mode_Should_Use_Route_Preference_On_Maps(int startX, int startY, int destinationX, int destinationY, string path)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(startX, startY),
                Destination = new Coordinates(destinationX, destinationY),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY}
                }),
                NavigationMode = CrawlerNavigationMode.Scout,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            var response = await crawler.Navigate();

            response.Arrived.Should().BeTrue();
            response.PathTaken.Should().Be(path);
        }

        [Fact]
        public async void Navigate_On_Scout_Mode_Should_BackTrack_To_Alternate_Routes()
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(0, 0),
                Destination = new Coordinates(1, 7),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.OCCPD, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY}
                }),
                NavigationMode = CrawlerNavigationMode.Scout,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            var response = await crawler.Navigate();

            response.Arrived.Should().BeTrue();
            response.PathTaken.Should().Be("EESSSSSSSW");
        }

        [Theory]
        [InlineData(0, 0, 2, 1, "EES")]
        [InlineData(0, 0, 1, 2, "SSE")]
        [InlineData(3, 3, 2, 1, "NNW")]
        [InlineData(3, 3, 1, 2, "WWN")]
        [InlineData(3, 5, 2, 3, "NWN")]
        [InlineData(3, 5, 1, 4, "WWN")]
        [InlineData(0, 3, 2, 4, "EES")]
        [InlineData(0, 3, 1, 5, "SES")]
        public async void Navigate_On_Scout_Mode_Should_Recalibrate_After_Each_Fork(int startX, int startY, int destinationX, int destinationY, string expected)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(startX, startY),
                Destination = new Coordinates(destinationX, destinationY),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY, Map.EMPTY},
                    new [] {Map.EMPTY, Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.EMPTY},
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.EMPTY},
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.EMPTY}
                }),
                NavigationMode = CrawlerNavigationMode.Scout,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            var response = await crawler.Navigate();

            response.Arrived.Should().BeTrue();
            response.PathTaken.Should().Be(expected);
        }

        [Fact]
        public async void Navigate_On_Scout_Mode_Should_Handle_Duplicate_Alternate_Routes()
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(2, 0),
                Destination = new Coordinates(6, 1),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY, Map.EMPTY, Map.OCCPD, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY, Map.OCCPD, Map.OCCPD, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.OCCPD, Map.OCCPD, Map.OCCPD, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.EMPTY}
                }),
                NavigationMode = CrawlerNavigationMode.Scout,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            var response = await crawler.Navigate();

            response.Arrived.Should().BeTrue();
            response.PathTaken.Should().Be("WWSSSSEEEEEENNN");
        }

        [Theory]
        [InlineData(0, 0, 2, 2)]
        [InlineData(0, 3, 2, 1)]
        public async void Navigate_On_Swarm_Mode_Should_Call_IMazeCrawlerCoordinator_Debrief(int startX, int startY, int destinationX, int destinationY)
        {
            var task = Task.Run(() => { Task.Delay(500); return new NavigationDetails { Arrived = false }; });
            var crawler = Substitute.For<IMazeCrawler>();
            crawler.Navigate().Returns(task);

            var crawlers = new [] {crawler};

            var swarmCoordinator = Substitute.For<ISwarmCoordinator>();
            swarmCoordinator.GetSwarm(Arg.Any<IMazeCrawlerState>()).Returns(crawlers);

            var crawlerCoordinator = Substitute.For<IMazeCrawlerCoordinator>();
            crawlerCoordinator.RequestSwarm(Arg.Any<IMazeCrawlerState>()).Returns(swarmCoordinator);

            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(startX, startY),
                Destination = new Coordinates(destinationX, destinationY),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY},
                    new [] {Map.OCCPD, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY}
                }),
                NavigationMode = CrawlerNavigationMode.Swarm,
                Coordinator = crawlerCoordinator
            };
            var mazeCrawler = new MazeCrawler(context);

            await mazeCrawler.Navigate();

            crawlerCoordinator.Received(1).Debrief(mazeCrawler);
        }

        [Theory]
        [InlineData(0, 0, 2, 2, 0)]
        [InlineData(0, 3, 2, 1, 1)]
        public async void Navigate_On_Swarm_Mode_Should_Call_IMazeCrawlerCoordinator_RequestSwarm_Only_When_There_Are_Forks(int startX, int startY, int destinationX, int destinationY, int numberOfCalls)
        {
            var swarmCoordinator = Substitute.For<ISwarmCoordinator>();
            swarmCoordinator.GetSwarm(Arg.Any<IMazeCrawlerState>()).Returns(new IMazeCrawler[0]);

            var crawlerCoordinator = Substitute.For<IMazeCrawlerCoordinator>();
            crawlerCoordinator.RequestSwarm(Arg.Any<IMazeCrawlerState>()).Returns(swarmCoordinator);

            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(startX, startY),
                Destination = new Coordinates(destinationX, destinationY),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY},
                    new [] {Map.OCCPD, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY}
                }),
                NavigationMode = CrawlerNavigationMode.Swarm,
                Coordinator = crawlerCoordinator
            };
            var crawler = new MazeCrawler(context);

            await crawler.Navigate();

            crawlerCoordinator.Received(numberOfCalls).RequestSwarm(crawler);
        }

        [Theory]
        [InlineData(0, 0, 2, 2, 0)]
        [InlineData(0, 3, 2, 1, 1)]
        public async void Navigate_On_Swarm_Mode_Should_Call_ISwarmCoordinator_GetSwarm_Only_When_There_Are_Forks(int startX, int startY, int destinationX, int destinationY, int numberOfCalls)
        {
            var swarmCoordinator = Substitute.For<ISwarmCoordinator>();
            swarmCoordinator.GetSwarm(Arg.Any<IMazeCrawlerState>()).Returns(new IMazeCrawler[0]);

            var crawlerCoordinator = Substitute.For<IMazeCrawlerCoordinator>();
            crawlerCoordinator.RequestSwarm(Arg.Any<IMazeCrawlerState>()).Returns(swarmCoordinator);

            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(startX, startY),
                Destination = new Coordinates(destinationX, destinationY),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY},
                    new [] {Map.OCCPD, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD, Map.EMPTY},
                    new [] {Map.EMPTY, Map.EMPTY, Map.EMPTY}
                }),
                NavigationMode = CrawlerNavigationMode.Swarm,
                Coordinator = crawlerCoordinator
            };
            var crawler = new MazeCrawler(context);

            await crawler.Navigate();

            swarmCoordinator.Received(numberOfCalls).GetSwarm(crawler);
        }

        [Fact]
        public async void Navigate_On_Swarm_Mode_Should_Call_Each_IMazeCrawler_Navigate()
        {
            var crawler1 = Substitute.For<IMazeCrawler>();
            crawler1.Navigate().Returns(Task.FromResult(new NavigationDetails()));
            var crawler2 = Substitute.For<IMazeCrawler>();
            crawler2.Navigate().Returns(Task.FromResult(new NavigationDetails()));
            var crawler3 = Substitute.For<IMazeCrawler>();
            crawler3.Navigate().Returns(Task.FromResult(new NavigationDetails()));
            var crawlers = new [] {crawler1, crawler2, crawler3};
            var swarmCoordinator = Substitute.For<ISwarmCoordinator>();
            swarmCoordinator.GetSwarm(Arg.Any<IMazeCrawlerState>()).Returns(crawlers);

            var crawlerCoordinator = Substitute.For<IMazeCrawlerCoordinator>();
            crawlerCoordinator.RequestSwarm(Arg.Any<IMazeCrawlerState>()).Returns(swarmCoordinator);

            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(0, 0),
                Destination = new Coordinates(0, 1),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD}
                }),
                NavigationMode = CrawlerNavigationMode.Swarm,
                Coordinator = crawlerCoordinator
            };
            var crawler = new MazeCrawler(context);

            await crawler.Navigate();

            await crawler1.Received(1).Navigate();
            await crawler2.Received(1).Navigate();
            await crawler3.Received(1).Navigate();
        }

        [Fact]
        public async void Navigate_On_Swarm_Mode_Should_Return_Correctly_When_All_Tasks_Completes_Without_Arrived_True_Result()
        {
            var task1 = Task.Run(() => { Task.Delay(300); return new NavigationDetails { Arrived = false }; });
            var crawler1 = Substitute.For<IMazeCrawler>();
            crawler1.Navigate().Returns(task1);

            var task2 = Task.Run(() => { Task.Delay(100); return new NavigationDetails { Arrived = false }; });
            var crawler2 = Substitute.For<IMazeCrawler>();
            crawler2.Navigate().Returns(task2);

            var task3 = Task.Run(() => { Task.Delay(200); return new NavigationDetails { Arrived = false }; });
            var crawler3 = Substitute.For<IMazeCrawler>();
            crawler3.Navigate().Returns(task3);

            var crawlers = new [] {crawler1, crawler2, crawler3};

            var swarmCoordinator = Substitute.For<ISwarmCoordinator>();
            swarmCoordinator.GetSwarm(Arg.Any<IMazeCrawlerState>()).Returns(crawlers);

            var crawlerCoordinator = Substitute.For<IMazeCrawlerCoordinator>();
            crawlerCoordinator.RequestSwarm(Arg.Any<IMazeCrawlerState>()).Returns(swarmCoordinator);

            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(0, 0),
                Destination = new Coordinates(0, 1),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD}
                }),
                NavigationMode = CrawlerNavigationMode.Swarm,
                Coordinator = crawlerCoordinator
            };
            var crawler = new MazeCrawler(context);

            var response = await crawler.Navigate();

            response.Arrived.Should().BeFalse();
        }

        [Fact]
        public async void Navigate_On_Swarm_Mode_Should_Return_Correctly_When_One_Task_Completes_With_Arrived_True_Result()
        {
            var result = new NavigationDetails { Arrived = true, PathTaken = "temp" };
            var task1 = Task.Run(() => { Task.Delay(200); return result; });
            var crawler1 = Substitute.For<IMazeCrawler>();
            crawler1.Navigate().Returns(task1);

            var task2 = Task.Run(() => { Task.Delay(300); return new NavigationDetails { Arrived = false }; });
            var crawler2 = Substitute.For<IMazeCrawler>();
            crawler2.Navigate().Returns(task2);

            var task3 = Task.Run(() => { Task.Delay(100); return new NavigationDetails { Arrived = false }; });
            var crawler3 = Substitute.For<IMazeCrawler>();
            crawler3.Navigate().Returns(task3);

            var crawlers = new [] {crawler1, crawler2, crawler3};

            var swarmCoordinator = Substitute.For<ISwarmCoordinator>();
            swarmCoordinator.GetSwarm(Arg.Any<IMazeCrawlerState>()).Returns(crawlers);

            var crawlerCoordinator = Substitute.For<IMazeCrawlerCoordinator>();
            crawlerCoordinator.RequestSwarm(Arg.Any<IMazeCrawlerState>()).Returns(swarmCoordinator);

            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(0, 0),
                Destination = new Coordinates(0, 1),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD}
                }),
                NavigationMode = CrawlerNavigationMode.Swarm,
                Coordinator = crawlerCoordinator
            };
            var crawler = new MazeCrawler(context);

            var response = await crawler.Navigate();

            response.Should().BeEquivalentTo(result);
        }

        [Fact]
        public async void Navigate_On_Swarm_Mode_Should_Return_The_First_Task_That_Completes_With_Arrived_True_Result()
        {
            var result1 = new NavigationDetails { Arrived = true, PathTaken = "result1" };
            var task1 = Task.Run(() => { Task.Delay(300); return result1; });
            var crawler1 = Substitute.For<IMazeCrawler>();
            crawler1.Navigate().Returns(task1);

            var result2 = new NavigationDetails { Arrived = true, PathTaken = "result2" };
            var task2 = Task.Run(() => { Task.Delay(500); return result2; });
            var crawler2 = Substitute.For<IMazeCrawler>();
            crawler2.Navigate().Returns(task2);

            var task3 = Task.Run(() => { Task.Delay(100); return new NavigationDetails { Arrived = false }; });
            var crawler3 = Substitute.For<IMazeCrawler>();
            crawler3.Navigate().Returns(task3);

            var crawlers = new [] {crawler1, crawler2, crawler3};

            var swarmCoordinator = Substitute.For<ISwarmCoordinator>();
            swarmCoordinator.GetSwarm(Arg.Any<IMazeCrawlerState>()).Returns(crawlers);

            var crawlerCoordinator = Substitute.For<IMazeCrawlerCoordinator>();
            crawlerCoordinator.RequestSwarm(Arg.Any<IMazeCrawlerState>()).Returns(swarmCoordinator);

            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(0, 0),
                Destination = new Coordinates(0, 1),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY},
                    new [] {Map.EMPTY, Map.OCCPD}
                }),
                NavigationMode = CrawlerNavigationMode.Swarm,
                Coordinator = crawlerCoordinator
            };
            var crawler = new MazeCrawler(context);

            var response = await crawler.Navigate();

            response.Should().BeEquivalentTo(result1);
        }

        [Theory]
        [InlineData(2, 2, true, "ENWWS")]
        [InlineData(2, 1, true, "ES")]
        [InlineData(3, 3, true, "NWWS")]
        [InlineData(1, 3, true, "ENW")]
        [InlineData(2, 2, false, "ENWWS")]
        [InlineData(2, 1, false, "ENWWS")]
        [InlineData(3, 3, false, "ENWWS")]
        [InlineData(1, 3, false, "ENWWS")]
        public void TraceSteps_Should_Return_Correctly(int startX, int startY, bool checkLegality, string expected)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(startX, startY),
                Destination = new Coordinates(5, 5),
                NavigationMap = _canMoveMap,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.MoveEast(checkLegality)
                   .MoveNorth(checkLegality)
                   .MoveWest(checkLegality)
                   .MoveWest(checkLegality)
                   .MoveSouth(checkLegality);

            crawler.TraceSteps().Should().Be(expected);
        }

        [Theory]
        [InlineData(3, 1, Direction.North, false)]
        [InlineData(2, 2, Direction.North, true)]
        [InlineData(4, 0, Direction.South, false)]
        [InlineData(1, 1, Direction.South, true)]
        [InlineData(1, 3, Direction.East, true)]
        [InlineData(0, 0, Direction.East, false)]
        [InlineData(3, 3, Direction.West, true)]
        [InlineData(0, 4, Direction.West, false)]
        public void CanMove_Should_Return_Correctly(int startX, int startY, Direction direction, bool expected)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(startX, startY),
                NavigationMap = _canMoveMap,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);
            crawler.CanMove(direction).Should().Be(expected);
        }

        [Theory]
        [InlineData(Direction.North, 2, 1, true, 2, 1)]
        [InlineData(Direction.North, 2, 1, false, 2, 0)]
        [InlineData(Direction.South, 1, 2, true, 1, 2)]
        [InlineData(Direction.South, 1, 2, false, 1, 3)]
        [InlineData(Direction.East, 2, 2, true, 2, 2)]
        [InlineData(Direction.East, 2, 2, false, 3, 2)]
        [InlineData(Direction.West, 1, 1, true, 1, 1)]
        [InlineData(Direction.West, 1, 1, false, 0, 1)]
        public void Move_Should_Move_Current_Coordinate_Appropriately(Direction direction, int currentX, int currentY, bool checkLegality, int expectedX, int expectedY)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(currentX, currentY),
                NavigationMap = _moveMap,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.Move(direction, checkLegality);

            crawler.CurrentX.Should().Be(expectedX);
            crawler.CurrentY.Should().Be(expectedY);
        }

        [Theory]
        [InlineData(1, 1, true, 1, 1)]
        [InlineData(1, 1, false, 1, 0)]
        [InlineData(1, 2, true, 1, 1)]
        [InlineData(1, 2, false, 1, 1)]
        [InlineData(2, 1, true, 2, 1)]
        [InlineData(2, 1, false, 2, 0)]
        [InlineData(2, 2, true, 2, 1)]
        [InlineData(2, 2, false, 2, 1)]
        public void MoveNorth_Should_Move_Current_Coordinate_Appropriately(int currentX, int currentY, bool checkLegality, int expectedX, int expectedY)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(currentX, currentY),
                NavigationMap = _moveMap,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.MoveNorth(checkLegality);

            crawler.CurrentX.Should().Be(expectedX);
            crawler.CurrentY.Should().Be(expectedY);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        public void MoveNorth_Should_Default_Unsupplied_Parameter_To_True(int y, int expectedY)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(0, y),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY},
                    new [] {Map.EMPTY}
                }),
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.MoveNorth();

            crawler.CurrentY.Should().Be(expectedY);
        }

        [Fact]
        public void MoveNorth_Should_Not_BackTrack()
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(0, 1),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY},
                    new [] {Map.EMPTY}
                }),
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.MoveNorth();
            crawler.MoveSouth();

            crawler.CanMove(Direction.South).Should().BeFalse();
            crawler.CurrentY.Should().Be(0);
        }

        [Theory]
        [InlineData(1, 1, true, 1, 2)]
        [InlineData(1, 1, false, 1, 2)]
        [InlineData(1, 2, true, 1, 2)]
        [InlineData(1, 2, false, 1, 3)]
        [InlineData(2, 1, true, 2, 2)]
        [InlineData(2, 1, false, 2, 2)]
        [InlineData(2, 2, true, 2, 2)]
        [InlineData(2, 2, false, 2, 3)]
        public void MoveSouth_Should_Move_Current_Coordinate_Appropriately(int currentX, int currentY, bool checkLegality, int expectedX, int expectedY)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(currentX, currentY),
                NavigationMap = _moveMap,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.MoveSouth(checkLegality);

            crawler.CurrentX.Should().Be(expectedX);
            crawler.CurrentY.Should().Be(expectedY);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        public void MoveSouth_Should_Default_Unsupplied_Parameter_To_True(int y, int expectedY)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(0, y),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY},
                    new [] {Map.EMPTY}
                }),
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.MoveSouth();

            crawler.CurrentY.Should().Be(expectedY);
        }

        [Fact]
        public void MoveSouth_Should_Not_BackTrack()
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(0, 0),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY},
                    new [] {Map.EMPTY}
                }),
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.MoveSouth();
            crawler.MoveNorth();

            crawler.CanMove(Direction.North).Should().BeFalse();
            crawler.CurrentY.Should().Be(1);
        }

        [Theory]
        [InlineData(1, 1, true, 2, 1)]
        [InlineData(1, 1, false, 2, 1)]
        [InlineData(1, 2, true, 2, 2)]
        [InlineData(1, 2, false, 2, 2)]
        [InlineData(2, 1, true, 2, 1)]
        [InlineData(2, 1, false, 3, 1)]
        [InlineData(2, 2, true, 2, 2)]
        [InlineData(2, 2, false, 3, 2)]
        public void MoveEast_Should_Move_Current_Coordinate_Appropriately(int currentX, int currentY, bool checkLegality, int expectedX, int expectedY)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(currentX, currentY),
                NavigationMap = _moveMap,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.MoveEast(checkLegality);

            crawler.CurrentX.Should().Be(expectedX);
            crawler.CurrentY.Should().Be(expectedY);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        public void MoveEast_Should_Default_Unsupplied_Parameter_To_True(int x, int expectedX)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(x, 0),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY}
                }),
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.MoveEast();

            crawler.CurrentX.Should().Be(expectedX);
        }

        [Fact]
        public void MoveEast_Should_Not_BackTrack()
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(0, 0),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY}
                }),
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.MoveEast();
            crawler.MoveWest();

            crawler.CanMove(Direction.West).Should().BeFalse();
            crawler.CurrentX.Should().Be(1);
        }

        [Theory]
        [InlineData(1, 1, true, 1, 1)]
        [InlineData(1, 1, false, 0, 1)]
        [InlineData(1, 2, true, 1, 2)]
        [InlineData(1, 2, false, 0, 2)]
        [InlineData(2, 1, true, 1, 1)]
        [InlineData(2, 1, false, 1, 1)]
        [InlineData(2, 2, true, 1, 2)]
        [InlineData(2, 2, false, 1, 2)]
        public void MoveWest_Should_Move_Current_Coordinate_Appropriately(int currentX, int currentY, bool checkLegality, int expectedX, int expectedY)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(currentX, currentY),
                NavigationMap = _moveMap,
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.MoveWest(checkLegality);

            crawler.CurrentX.Should().Be(expectedX);
            crawler.CurrentY.Should().Be(expectedY);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        public void MoveWest_Should_Default_Unsupplied_Parameter_To_True(int x, int expectedX)
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(x, 0),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY}
                }),
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.MoveWest();

            crawler.CurrentX.Should().Be(expectedX);
        }

        [Fact]
        public void MoveWest_Should_Not_BackTrack()
        {
            var context = new MazeCrawlerContext
            {
                Start = new Coordinates(1, 0),
                NavigationMap = new Map(new char[][]
                {
                    new [] {Map.EMPTY, Map.EMPTY}
                }),
                Coordinator = Substitute.For<IMazeCrawlerCoordinator>()
            };
            var crawler = new MazeCrawler(context);

            crawler.MoveWest();
            crawler.MoveEast();

            crawler.CanMove(Direction.East).Should().BeFalse();
            crawler.CurrentX.Should().Be(0);
        }
    }
}