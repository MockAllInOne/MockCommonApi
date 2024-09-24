using MockAllInOne.MockingModel.Model;
using MockAllInOne.MockingModel.Model.Interfaces;
using System.Text;

namespace MockAllInOne.ClientMock
{
    public enum HttpMethodType
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    public class MockClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public MockClient()
        {
            _httpClient = new HttpClient(); // Socket exhaustion
        }

        // return IMockMessage ?
        public async Task<IMockMessage> Send(string address, string body, Dictionary<string, string> headers, HttpMethodType method, string mediaType="application/xml")
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(address),
                Content = new StringContent(body, Encoding.UTF8, mediaType)
            };

            SetHttpMethodFor(request, method);
            SetupHttpHeader(request, headers);
            
            var response = await _httpClient.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseHeaders = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

            return new MMessage(MessageType.Response, headers, responseBody);
        }

        private void SetHttpMethodFor(HttpRequestMessage request, HttpMethodType method)
        {
            switch (method)
            {
                case HttpMethodType.GET:
                    request.Method = HttpMethod.Get;
                    request.Content = null;
                    break;
                case HttpMethodType.POST:
                    request.Method = HttpMethod.Post;
                    break;
                case HttpMethodType.PUT:
                    request.Method = HttpMethod.Put;
                    break;
                case HttpMethodType.DELETE:
                    request.Method = HttpMethod.Delete;
                    request.Content = null;
                    break;
                default:
                    throw new ArgumentException("Unsupported HTTP method.");
            }
        }

        private void SetupHttpHeader(HttpRequestMessage request, Dictionary<string, string> headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
