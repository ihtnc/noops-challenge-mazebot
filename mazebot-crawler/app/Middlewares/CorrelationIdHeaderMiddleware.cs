using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MazebotCrawler.Middlewares
{
    public class CorrelationIdHeaderMiddleware
    {
        public const string CORRELATION_ID = "correlation-id";

        private readonly RequestDelegate _next;

        public CorrelationIdHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            AddRequestTracking(context);
            AddResponseTracking(context);

            await _next(context);
        }

        private void AddRequestTracking(HttpContext context)
        {
            var request = context.Request;
            if(!request.Headers.ContainsKey(CORRELATION_ID))
            {
                request.Headers[CORRELATION_ID] = Guid.NewGuid().ToString();
            }
        }

        private void AddResponseTracking(HttpContext context)
        {
            var response = context.Response;

            // expose correlation-id header (if not yet exposed)
            var accessControlExposeHeaders = "Access-Control-Expose-Headers";
            IEnumerable<string> exposedHeaders = response.Headers[accessControlExposeHeaders];
            if(!exposedHeaders.Any(header => header.Equals(CORRELATION_ID, StringComparison.OrdinalIgnoreCase)))
            {
                exposedHeaders = exposedHeaders.Append(CORRELATION_ID);
            }
            response.Headers[accessControlExposeHeaders] = new StringValues(exposedHeaders.ToArray());

            // persist the correlation-id in the request to the response
            response.Headers[CORRELATION_ID] = context.Request.Headers[CORRELATION_ID];
        }
    }
}