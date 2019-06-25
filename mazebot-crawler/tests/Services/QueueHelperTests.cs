using System.Collections.Generic;
using MazebotCrawler.Services;
using Xunit;
using FluentAssertions;
using FluentAssertions.Collections;

namespace MazebotCrawler.Tests.Services
{
    public class QueueHelperTests
    {
        [Fact]
        public void AddQueue_Should_Return_Correctly()
        {
            var list1 = new int[] {1, 9, 5};
            var queue1 = new Queue<int>();
            queue1.Enqueue(list1[0]);
            queue1.Enqueue(list1[1]);
            queue1.Enqueue(list1[2]);

            var list2 = new int[] {99, 3};
            var queue2 = new Queue<int>();
            queue2.Enqueue(list2[0]);
            queue2.Enqueue(list2[1]);

            var newQueue = QueueHelper.AddQueue(queue1, queue2);
            newQueue.Should().StartWith(list1);
            newQueue.Should().EndWith(list2);
        }

        [Fact]
        public void AddQueue_Should_Handle_Null_Source()
        {
            var list = new int[] {99, 3};
            var queue = new Queue<int>();
            queue.Enqueue(list[0]);
            queue.Enqueue(list[1]);

            var newQueue = QueueHelper.AddQueue(null, queue);
            newQueue.Should().BeNull();
        }

        [Fact]
        public void AddQueue_Should_Handle_Null_QueueToAdd()
        {
            var list = new int[] {1, 9, 5};
            var queue = new Queue<int>();
            queue.Enqueue(list[0]);
            queue.Enqueue(list[1]);
            queue.Enqueue(list[2]);

            var newQueue = QueueHelper.AddQueue(queue, null);
            newQueue.Should().StartWith(list);
            newQueue.Should().EndWith(list);
        }
    }
}