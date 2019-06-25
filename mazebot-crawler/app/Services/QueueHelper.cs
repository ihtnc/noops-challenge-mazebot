using System.Collections.Generic;
using System.Linq;

namespace MazebotCrawler.Services
{
    public class QueueHelper
    {
        public static Queue<T> AddQueue<T>(Queue<T> queue, Queue<T> additional)
        {
            while (queue != null && additional?.Count > 0) { queue.Enqueue(additional.Dequeue()); }
            return queue;
        }
    }
}