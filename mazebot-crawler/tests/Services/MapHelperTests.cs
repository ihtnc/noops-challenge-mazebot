using MazebotCrawler.Services;
using MazebotCrawler.Services.Models;
using Xunit;
using FluentAssertions;

namespace MazebotCrawler.Tests.Services
{
    public class MapHelperTests
    {
        private readonly Map _canMoveMap;

        public MapHelperTests()
        {
            _canMoveMap = new Map(new char[][]
            {
                new [] {Map.EMPTY, Map.OCCPD, Map.OCCPD, Map.OCCPD, Map.EMPTY},
                new [] {Map.OCCPD, Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.OCCPD},
                new [] {Map.OCCPD, Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.OCCPD},
                new [] {Map.OCCPD, Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.OCCPD},
                new [] {Map.EMPTY, Map.OCCPD, Map.OCCPD, Map.OCCPD, Map.EMPTY}
            });
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
            var start = new Coordinates(startX, startY);
            MapHelper.CanMove(_canMoveMap, start, direction).Should().Be(expected);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(4, 0, false)]
        [InlineData(1, 1, false)]
        [InlineData(3, 1, false)]
        [InlineData(2, 2, true)]
        [InlineData(3, 3, true)]
        [InlineData(1, 3, true)]
        [InlineData(0, 4, false)]
        [InlineData(4, 4, false)]
        public void CanMoveNorth_Should_Return_Correctly(int startX, int startY, bool expected)
        {
            var start = new Coordinates(startX, startY);
            MapHelper.CanMoveNorth(_canMoveMap, start).Should().Be(expected);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(4, 0, false)]
        [InlineData(1, 1, true)]
        [InlineData(3, 1, true)]
        [InlineData(2, 2, true)]
        [InlineData(1, 3, false)]
        [InlineData(3, 3, false)]
        [InlineData(0, 4, false)]
        [InlineData(4, 4, false)]
        public void CanMoveSouth_Should_Return_Correctly(int startX, int startY, bool expected)
        {
            var start = new Coordinates(startX, startY);
            MapHelper.CanMoveSouth(_canMoveMap, start).Should().Be(expected);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(4, 0, false)]
        [InlineData(1, 1, true)]
        [InlineData(3, 1, false)]
        [InlineData(2, 2, true)]
        [InlineData(1, 3, true)]
        [InlineData(3, 3, false)]
        [InlineData(0, 4, false)]
        [InlineData(4, 4, false)]
        public void CanMoveEast_Should_Return_Correctly(int startX, int startY, bool expected)
        {
            var start = new Coordinates(startX, startY);
            MapHelper.CanMoveEast(_canMoveMap, start).Should().Be(expected);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(4, 0, false)]
        [InlineData(1, 1, false)]
        [InlineData(3, 1, true)]
        [InlineData(2, 2, true)]
        [InlineData(1, 3, false)]
        [InlineData(3, 3, true)]
        [InlineData(0, 4, false)]
        [InlineData(4, 4, false)]
        public void CanMoveWest_Should_Return_Correctly(int startX, int startY, bool expected)
        {
            var start = new Coordinates(startX, startY);
            MapHelper.CanMoveWest(_canMoveMap, start).Should().Be(expected);
        }

        [Fact]
        public void ConvertToString_Should_Return_Correctly()
        {
            var floorPlan = new char[][]
            {
                new char[] {Map.OCCPD, Map.EMPTY, Map.START, Map.DESTN},
                new char[] {Map.EMPTY, Map.EMPTY, Map.OCCPD, Map.OCCPD},
                new char[] {Map.EMPTY, Map.EMPTY, Map.EMPTY, Map.EMPTY}
            };

            var expected = $"[{Map.OCCPD}][{Map.EMPTY}][{Map.START}][{Map.DESTN}]\n[{Map.EMPTY}][{Map.EMPTY}][{Map.OCCPD}][{Map.OCCPD}]\n[{Map.EMPTY}][{Map.EMPTY}][{Map.EMPTY}][{Map.EMPTY}]\n";

            var actual = MapHelper.ConvertToString(floorPlan);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("EENES", "EEE")]
        [InlineData("NWNWS", "NWW")]
        [InlineData("SENNN", "ENN")]
        [InlineData("SSWNW", "SWW")]
        [InlineData("SESWS", "SSS")]
        [InlineData("SWSES", "SSS")]
        [InlineData("NEESENENWNWSWWWSS", "W")]
        [InlineData("ENENENNWNWNNWWSS", "ENENENNWNWNNWWSS")]
        [InlineData("Unknown format", "Unknown format")]
        [InlineData(null, null)]
        public void SimplifyPath_Should_Return_Correctly(string path, string expected)
        {
            var actual = MapHelper.SimplifyPath(path);
            actual.Should().Be(expected);
        }
    }
}