using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace MockAllInOne.ServiceMock
{
    public class HttpMock
    {
        public bool IsStarted { get; private set; }
        private readonly HttpListener _httpListener;
        private readonly IMockSettings _mockSettings;
        private readonly CancellationTokenSource _cancellationToken = new();

        public HttpMock(IMockSettings mockSettings)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("HttpListener class is not supported on this machne.");

            _mockSettings = mockSettings ?? throw new ArgumentNullException(nameof(mockSettings));
            _httpListener = new HttpListener();
        }

        public void Start()
        {
            Task.Run(() =>
            {
                string prefix = $"http://*:{_mockSettings.Port}/{_mockSettings.Path}/";
                _httpListener.Prefixes.Add(prefix);
                _httpListener.Start();

                Console.WriteLine("Listening..");
                IsStarted = true;

                while (!_cancellationToken.Token.IsCancellationRequested)
                {
                    IAsyncResult result = _httpListener.BeginGetContext(OnContextReceived, null);
                    result.AsyncWaitHandle.WaitOne();
                }
            });

        }

        public void Stop()
        {
            _cancellationToken.Cancel();
            _httpListener.Stop();
            _httpListener.Close();
            IsStarted = false;

            Console.WriteLine("Service stopped.");// TODO: add logger..
        }

        private void OnContextReceived(IAsyncResult result)
        {
            try
            {
                HttpListenerContext context = _httpListener.EndGetContext(result);
                Task.Run(() => HandleRequest(context));
            }
            catch (Exception ex) { Console.WriteLine(ex); }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            Console.WriteLine($"Received request: {context.Request.HttpMethod} {context.Request.Url}");

            // Read query parameters TODO: move all header data
            var queryParams = context.Request.QueryString;
            PrintQueryParams(queryParams);

            // Read headers
            var headerValues = context.Request.Headers;
            Console.WriteLine($"Header Values: \n {string.Join(",", headerValues)}");

            // Read the request body
            string requestBody;
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                requestBody = reader.ReadToEnd();
                Console.WriteLine($"Request Body: {requestBody}");
            }

            string responseString = "<html><body>Hello, World!</body></html>";
            byte[] responseBuffer = Encoding.UTF8.GetBytes(responseString);

            // Prepare the response
            context.Response.ContentLength64 = responseBuffer.Length;
            context.Response.ContentType = "text/html";
            context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
            context.Response.OutputStream.Close();

            Console.WriteLine($"Handled request from {request.RemoteEndPoint}.");
        }

        private void PrintQueryParams(NameValueCollection collection)
        {
            Console.WriteLine($"Query params:");
            foreach (string key in collection.AllKeys)
            {
                Console.WriteLine($"Key: {key}");

                foreach (string value in collection.GetValues(key))
                {
                    Console.WriteLine($"    Value: {value}");
                }
            }
        }
    }
}
