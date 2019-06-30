using Microsoft.Extensions.Logging;
using MazebotCrawler.Crawlies;
using MazebotCrawler.Crawlies.Models;
using MazebotCrawler.Services.Models;
using Xunit;
using FluentAssertions;
using NSubstitute;

namespace MazebotCrawler.Tests.Crawlies
{
    public class MazeCrawlerQueenTests
    {
        private readonly IMazeCrawlerSpawner _spawner;
        private readonly MazeCrawlerQueen _queen;

        public MazeCrawlerQueenTests()
        {
            _spawner = Substitute.For<IMazeCrawlerSpawner>();
            _queen = new MazeCrawlerQueen(_spawner, Substitute.For<ILogger<MazeCrawlerQueen>>());
        }

        [Fact]
        public void Constructor_Should_Set_Id()
        {
            _queen.Id.Should().NotBeNull();
        }

        [Fact]
        public void ScanMap_Should_Set_Properties_Correctly()
        {
            var start = new Coordinates(1, 2);
            var destination = new Coordinates(3, 4);
            var floorPlan = new char[1][]
            {
                new char[] {Map.OCCPD, Map.EMPTY, Map.START, Map.DESTN}
            };
            var map = new Map(floorPlan);

            _queen.ScanMap(start, destination, map);

            _queen.StartX.Should().Be(start.X);
            _queen.StartY.Should().Be(start.Y);
            _queen.DestinationX.Should().Be(destination.X);
            _queen.DestinationY.Should().Be(destination.Y);
            _queen.Map.FloorPlan.Should().BeEquivalentTo(floorPlan, o => o.WithStrictOrdering());
        }

        [Fact]
        public async void Navigate_Should_Call_IMazeSpawner_Spawn()
        {
            var start = new Coordinates(1, 2);
            var destination = new Coordinates(3, 4);
            var floorPlan = new char[0][];
            var map = new Map(floorPlan);

            _queen.ScanMap(start, destination, map);

            var crawler = Substitute.For<IMazeCrawler>();
            crawler.Navigate().Returns(new NavigationDetails());

            MazeCrawlerContext context = null;
            _spawner.Spawn(Arg.Do<MazeCrawlerContext>(c => context = c)).Returns(crawler);

            await _queen.Navigate();

            _spawner.Received(1).Spawn(Arg.Any<MazeCrawlerContext>());
            context.Start.Should().Be(start);
            context.Destination.Should().Be(destination);
            context.NavigationMode.Should().Be(CrawlerNavigationMode.Scout);
            context.Coordinator.Should().Be(_queen);
        }

        [Fact]
        public async void Navigate_Should_Mask_Map_Passed_In_MazeCrawlerContext()
        {
            var floorPlan = new char[1][]
            {
                new char[] {Map.OCCPD, Map.MOVEW, Map.START, Map.DESTN}
            };
            var map = new Map(floorPlan);
            var maskedFloorPlan = new char[1][]
            {
                new char[] {Map.OCCPD, Map.OCCPD, Map.EMPTY, Map.EMPTY}
            };

            _queen.ScanMap(new Coordinates(1, 2), new Coordinates(3, 4), map);

            var crawler = Substitute.For<IMazeCrawler>();
            crawler.Navigate().Returns(new NavigationDetails());

            MazeCrawlerContext context = null;
            _spawner.Spawn(Arg.Do<MazeCrawlerContext>(c => context = c)).Returns(crawler);

            await _queen.Navigate();

            _spawner.Received(1).Spawn(Arg.Any<MazeCrawlerContext>());
            context.NavigationMap.FloorPlan.Should().BeEquivalentTo(maskedFloorPlan, o => o.WithStrictOrdering());
        }

        [Fact]
        public async void Navigate_Should_Call_Spawned_IMazeCrawler_Navigate()
        {
            _queen.ScanMap(new Coordinates(1, 2), new Coordinates(3, 4), new Map(new char[0][]));

            var expected = new NavigationDetails();
            var crawler = Substitute.For<IMazeCrawler>();
            crawler.Navigate().Returns(expected);
            _spawner.Spawn(Arg.Any<MazeCrawlerContext>()).Returns(crawler);

            var response = await _queen.Navigate();

            await crawler.Received(1).Navigate();
            response.Should().Be(expected);
        }

        [Theory]
        [InlineData(true, "NNESE", "NEE")]
        [InlineData(false, "NNESE", "NNESE")]
        public async void Navigate_Should_Simplify_Path_Taken_By_Crawler_If_It_Arrived(bool arrived, string pathTaken, string expectedPath)
        {
            _queen.ScanMap(new Coordinates(1, 2), new Coordinates(3, 4), new Map(new char[0][]));

            var crawler = Substitute.For<IMazeCrawler>();
            crawler.Navigate().Returns(new NavigationDetails
            {
                Arrived = arrived,
                PathTaken = pathTaken
            });
            _spawner.Spawn(Arg.Any<MazeCrawlerContext>()).Returns(crawler);

            var response = await _queen.Navigate();

            response.PathTaken.Should().Be(expectedPath);
        }

        [Fact]
        public void RequestSwarm_Should_Update_Map_Based_On_CrawlerMap()
        {
            var floorPlan = new char[1][]
            {
                new char[] {Map.OCCPD, Map.EMPTY, Map.START, Map.DESTN}
            };
            var map = new Map(floorPlan);

            _queen.ScanMap(new Coordinates(1, 2), new Coordinates(3, 4), map);

            var crawlerFloorPlan = new char[1][]
            {
                new char[] {Map.OCCPD, Map.MOVEE, Map.START, Map.DESTN}
            };
            var crawlerMap = new Map(crawlerFloorPlan);
            var crawlerState = Substitute.For<IMazeCrawlerState>();
            crawlerState.CrawlerMap.Returns(crawlerMap);

            _queen.RequestSwarm(crawlerState);

            _queen.Map.FloorPlan.Should().BeEquivalentTo(crawlerFloorPlan, o => o.WithStrictOrdering());
        }

        [Fact]
        public void RequestSwarm_Should_Return_Correctly()
        {
            var floorPlan = new char[1][]
            {
                new char[] {Map.OCCPD, Map.EMPTY, Map.START, Map.DESTN}
            };
            var map = new Map(floorPlan);

            _queen.ScanMap(new Coordinates(1, 2), new Coordinates(3, 4), map);

            var crawlerState = Substitute.For<IMazeCrawlerState>();
            crawlerState.CrawlerMap.Returns(map);

            var coordinator = _queen.RequestSwarm(crawlerState);

            coordinator.Should().Be(_queen);
        }

        [Fact]
        public void Debrief_Should_Update_Map_Based_On_CrawlerMap()
        {
            var floorPlan = new char[1][]
            {
                new char[] {Map.OCCPD, Map.EMPTY, Map.START, Map.DESTN}
            };
            var map = new Map(floorPlan);

            _queen.ScanMap(new Coordinates(1, 2), new Coordinates(3, 4), map);

            var crawlerFloorPlan = new char[1][]
            {
                new char[] {Map.OCCPD, Map.MOVEW, Map.START, Map.DESTN}
            };
            var crawlerMap = new Map(crawlerFloorPlan);
            var crawlerState = Substitute.For<IMazeCrawlerState>();
            crawlerState.CrawlerMap.Returns(crawlerMap);

            _queen.Debrief(crawlerState);

            _queen.Map.FloorPlan.Should().BeEquivalentTo(crawlerFloorPlan, o => o.WithStrictOrdering());
        }

        [Theory]
        [InlineData(Direction.North)]
        [InlineData(Direction.South)]
        [InlineData(Direction.East)]
        [InlineData(Direction.West)]
        public void GetSwarm_Should_Call_IMazeCrawlerSpawner_Spawn_Based_On_Requestor(Direction direction)
        {
            var start = new Coordinates(1, 2);
            var destination = new Coordinates(3, 4);
            var floorPlan = new char[1][] { new char[] {Map.OCCPD, Map.MOVEW, Map.START, Map.DESTN} };
            var map = new Map(floorPlan);
            var maskedFloorPlan = new char[1][] { new char[] {Map.OCCPD, Map.OCCPD, Map.EMPTY, Map.EMPTY} };

            _queen.ScanMap(start, destination, map);

            MazeCrawlerContext context = null;
            _spawner.Spawn(Arg.Do<MazeCrawlerContext>(c => context = c));

            var crawlerState = Substitute.For<IMazeCrawlerState>();
            crawlerState.CanMove(direction).Returns(true);
            crawlerState.CurrentX.Returns(8);
            crawlerState.CurrentY.Returns(9);
            crawlerState.NavigationMode.Returns(CrawlerNavigationMode.Swarm);

            _queen.GetSwarm(crawlerState);

            _spawner.Received(1).Spawn(Arg.Any<MazeCrawlerContext>());
            context.Start.X.Should().Be(crawlerState.CurrentX);
            context.Start.Y.Should().Be(crawlerState.CurrentY);
            context.Destination.Should().Be(destination);
            context.NavigationMap.FloorPlan.Should().BeEquivalentTo(maskedFloorPlan, o => o.WithStrictOrdering());
            context.NavigationMode.Should().Be(crawlerState.NavigationMode);
            context.Coordinator.Should().Be(_queen);
        }

        [Theory]
        [InlineData(Direction.North)]
        [InlineData(Direction.South)]
        [InlineData(Direction.East)]
        [InlineData(Direction.West)]
        public void GetSwarm_Should_Call_Spawned_IMazeCrawler_Move(Direction direction)
        {
            _queen.ScanMap(new Coordinates(1, 2), new Coordinates(3, 4), new Map(new char[0][]));

            var crawler = Substitute.For<IMazeCrawler>();
            _spawner.Spawn(Arg.Any<MazeCrawlerContext>()).Returns(crawler);

            var crawlerState = Substitute.For<IMazeCrawlerState>();
            crawlerState.CanMove(direction).Returns(true);

            _queen.GetSwarm(crawlerState);

            crawler.Received(1).Move(direction);
        }

        [Theory]
        [InlineData(true, false, false, false, 1)]
        [InlineData(true, false, true, false, 2)]
        [InlineData(false, false, false, false, 0)]
        [InlineData(false, true, true, true, 3)]
        [InlineData(true, true, true, true, 4)]
        public void GetSwarm_Should_Call_IMazeCrawler_Spawn_For_Each_Direction(bool canMoveNorth, bool canMoveSouth, bool canMoveEast, bool canMoveWest, int expectedCall)
        {
            _queen.ScanMap(new Coordinates(1, 2), new Coordinates(3, 4), new Map(new char[0][]));

            var crawler = Substitute.For<IMazeCrawler>();
            _spawner.Spawn(Arg.Any<MazeCrawlerContext>()).Returns(crawler);

            var crawlerState = Substitute.For<IMazeCrawlerState>();
            crawlerState.CanMove(Direction.North).Returns(canMoveNorth);
            crawlerState.CanMove(Direction.South).Returns(canMoveSouth);
            crawlerState.CanMove(Direction.East).Returns(canMoveEast);
            crawlerState.CanMove(Direction.West).Returns(canMoveWest);

            _queen.GetSwarm(crawlerState);

            _spawner.Received(expectedCall).Spawn(Arg.Any<MazeCrawlerContext>());
        }

        [Theory]
        [InlineData(true, false, false, false, 1, 0, 0, 0)]
        [InlineData(true, false, true, false, 1, 0, 1, 0)]
        [InlineData(false, false, false, false, 0, 0, 0, 0)]
        [InlineData(false, true, true, true, 0, 1, 1, 1)]
        [InlineData(true, true, true, true, 1, 1, 1, 1)]
        public void GetSwarm_Should_Call_Spawned_IMazeCrawler_Move_For_Each_Direction(bool canMoveNorth, bool canMoveSouth, bool canMoveEast, bool canMoveWest, int moveNorthCall, int moveSouthCall, int moveEastCall, int moveWestCall)
        {
            _queen.ScanMap(new Coordinates(1, 2), new Coordinates(3, 4), new Map(new char[0][]));

            var crawler = Substitute.For<IMazeCrawler>();
            _spawner.Spawn(Arg.Any<MazeCrawlerContext>()).Returns(crawler);

            var crawlerState = Substitute.For<IMazeCrawlerState>();
            crawlerState.CanMove(Direction.North).Returns(canMoveNorth);
            crawlerState.CanMove(Direction.South).Returns(canMoveSouth);
            crawlerState.CanMove(Direction.East).Returns(canMoveEast);
            crawlerState.CanMove(Direction.West).Returns(canMoveWest);

            _queen.GetSwarm(crawlerState);

            crawler.Received(moveNorthCall).Move(Direction.North);
            crawler.Received(moveSouthCall).Move(Direction.South);
            crawler.Received(moveEastCall).Move(Direction.East);
            crawler.Received(moveWestCall).Move(Direction.West);
        }

        [Theory]
        [InlineData(true, false, false, false, 1)]
        [InlineData(true, false, true, false, 2)]
        [InlineData(false, false, false, false, 0)]
        [InlineData(false, true, true, true, 3)]
        [InlineData(true, true, true, true, 4)]
        public void GetSwarm_Should_Return_Correctly(bool canMoveNorth, bool canMoveSouth, bool canMoveEast, bool canMoveWest, int swarmCount)
        {
            _queen.ScanMap(new Coordinates(1, 2), new Coordinates(3, 4), new Map(new char[0][]));

            _spawner.Spawn(Arg.Any<MazeCrawlerContext>()).Returns(Substitute.For<IMazeCrawler>());

            var crawlerState = Substitute.For<IMazeCrawlerState>();
            crawlerState.CanMove(Direction.North).Returns(canMoveNorth);
            crawlerState.CanMove(Direction.South).Returns(canMoveSouth);
            crawlerState.CanMove(Direction.East).Returns(canMoveEast);
            crawlerState.CanMove(Direction.West).Returns(canMoveWest);

            var swarm = _queen.GetSwarm(crawlerState);

            swarm.Should().HaveCount(swarmCount);
        }
    }
}