using System.Linq;
using MazebotCrawler.Services;
using MazebotCrawler.Services.Models;
using Xunit;
using FluentAssertions;

namespace MazebotCrawler.Tests.Services
{
    public class DirectionHelperTests
    {
        [Theory]
        [InlineData(3, 0, Direction.West, Direction.East)]
        [InlineData(1, 2, Direction.East, Direction.West)]
        public void GetPreferences_Should_Make_Preferences_Based_On_Horizontal_Component(int startX, int destinationX, Direction first, Direction last)
        {
            var start = new Coordinates(startX, 0);
            var destination = new Coordinates(destinationX, 0);

            var actual = DirectionHelper.GetPreferences(start, destination);

            actual.First().Should().Be(first);
            actual.Last().Should().Be(last);
        }

        [Theory]
        [InlineData(3, 0, Direction.North, Direction.South)]
        [InlineData(1, 2, Direction.South, Direction.North)]
        public void GetPreferences_Should_Make_Preferences_Based_On_Vertical_Component(int startY, int destinationY, Direction first, Direction last)
        {
            var start = new Coordinates(0, startY);
            var destination = new Coordinates(0, destinationY);

            var actual = DirectionHelper.GetPreferences(start, destination);

            actual.First().Should().Be(first);
            actual.Last().Should().Be(last);
        }

        [Fact]
        public void GetPreferences_Should_Prefer_North_If_Vertical_Component_Is_The_Same()
        {
            var start = new Coordinates(1, 1);
            var destination = new Coordinates(5, 1);

            var actual = DirectionHelper.GetPreferences(start, destination);

            actual[1].Should().Be(Direction.North);
            actual[2].Should().Be(Direction.South);
        }

        [Fact]
        public void GetPreferences_Should_Prefer_East_If_Horizontal_Component_Is_The_Same()
        {
            var start = new Coordinates(1, 5);
            var destination = new Coordinates(1, 1);

            var actual = DirectionHelper.GetPreferences(start, destination);

            actual[1].Should().Be(Direction.East);
            actual[2].Should().Be(Direction.West);
        }

        [Theory]
        [InlineData(4, 1, Direction.East, Direction.North)]
        [InlineData(5, 3, Direction.East, Direction.South)]
        [InlineData(4, 10, Direction.South, Direction.East)]
        [InlineData(3, 0, Direction.North, Direction.East)]
        [InlineData(0, 1, Direction.West, Direction.North)]
        [InlineData(0, 3, Direction.West, Direction.South)]
        [InlineData(1, 0, Direction.North, Direction.West)]
        [InlineData(1, 4, Direction.South, Direction.West)]
        public void GetPreferences_Should_Make_Preferences_Based_On_Distance(int destinationX, int destinationY, Direction first, Direction second)
        {
            var start = new Coordinates(2, 2);
            var destination = new Coordinates(destinationX, destinationY);

            var actual = DirectionHelper.GetPreferences(start, destination);

            actual[0].Should().Be(first);
            actual[1].Should().Be(second);
        }

        [Fact]
        public void GetPreferences_Should_Prefer_Horizontal_Component_If_Distance_Is_The_Same()
        {
            var start = new Coordinates(5, 5);
            var destination = new Coordinates(1, 1);

            var actual = DirectionHelper.GetPreferences(start, destination);

            actual[0].Should().Be(Direction.West);
            actual[1].Should().Be(Direction.North);
        }
    }
}