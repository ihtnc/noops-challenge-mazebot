using System.Linq;
using MazebotCrawler.Repositories;
using MazebotCrawler.Services.Models;
using Xunit;
using FluentAssertions;

namespace MazebotCrawler.Tests.Repositories
{
    public class InMemoryStatusRepositoryTests
    {
        [Fact]
        public async void Add_Should_Add_Status_Accordingly()
        {
            var repository = new InMemoryStatusRepository();

            var sessionId = "sessionId";
            var item = new MazebotSolverResponse(sessionId, null, null, null);

            var actual = await repository.Add(sessionId, item);

            var list = await repository.Get(sessionId);
            var expected = list.First();
            actual.SessionId.Should().Be(expected.SessionId);
            actual.Response.Should().Be(expected.Response);
        }

        [Theory]
        [InlineData("sessionId1", 1)]
        [InlineData("sessionId2", 2)]
        [InlineData("nonExisting", 0)]
        public async void Get_Should_Retrieve_Status_Accordingly(string sessionId, int recordCount)
        {
            var repository = new InMemoryStatusRepository();

            var item1 = new MazebotSolverResponse("sessionId1", null, null, null);
            var item2 = new MazebotSolverResponse("sessionId2", null, null, null);
            var item3 = new MazebotSolverResponse("sessionId2", null, null, null);

            await repository.Add("sessionId1", item1);
            await repository.Add("sessionId2", item2);
            await repository.Add("sessionId2", item3);

            var result = await repository.Get(sessionId);
            result.Should().HaveCount(recordCount);
        }
    }
}