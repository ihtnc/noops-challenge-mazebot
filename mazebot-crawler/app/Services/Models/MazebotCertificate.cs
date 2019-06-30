using System;

namespace MazebotCrawler.Services.Models
{
    public class MazebotCertificate
    {
        public string Message { get; set; }
        public decimal Elapsed { get; set; }
        public DateTimeOffset Completed { get; set; }
    }
}
