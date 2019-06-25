using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace MazebotCrawler.Services
{
    public interface IApiRequestProvider
    {
        HttpRequestMessage CreateGetRequest(string url, Dictionary<string, string> headers = null, Dictionary<string, string> queries = null);
        HttpRequestMessage CreatePostRequest<T>(string url, Dictionary<string, string> headers = null, T content = null, Dictionary<string, string> queries = null) where T : class;
        HttpRequestMessage CreateRequest(HttpMethod method, string url, Dictionary<string, string> headers = null, object content = null, Dictionary<string, string> queries = null);
    }

    public class ApiRequestProvider : IApiRequestProvider
    {
        public HttpRequestMessage CreateGetRequest(string url, Dictionary<string, string> headers = null, Dictionary<string, string> queries = null)
        {
            return CreateRequest(HttpMethod.Get, url, headers: headers, queries: queries);
        }

        public HttpRequestMessage CreatePostRequest<T>(string url, Dictionary<string, string> headers = null, T content = null, Dictionary<string, string> queries = null) where T : class
        {
            return CreateRequest(HttpMethod.Post, url, headers: headers, content: content, queries: queries);
        }

        public HttpRequestMessage CreateRequest(HttpMethod method, string url, Dictionary<string, string> headers = null, object content = null, Dictionary<string, string> queries = null)
        {
            var requestUrl = url;

            if (queries?.Count > 0)
            {
                var checkUri = new Uri(url);
                var urlHasQueryString = checkUri.Query?.StartsWith('?') == true;
                var list = queries.Select(q => $"{q.Key}={q.Value}");
                var concatenated = string.Join("&", list);
                var queryString = (urlHasQueryString ? "&" : "?") + concatenated;
                requestUrl = url + queryString;
            }

            var message = new HttpRequestMessage(method, requestUrl);

            if (headers?.Count > 0)
            {
                foreach(var item in headers)
                {
                    message.Headers.Add(item.Key, item.Value);
                }
            }

            if (content != null)
            {
                var body = JsonConvert.SerializeObject(content);
                message.Content = new StringContent(body);
                message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return message;
        }
    }
}