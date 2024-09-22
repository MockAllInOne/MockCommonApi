namespace MockAllInOne.ServiceMock
{
    public class MockSettings : IMockSettings
    {
        public int Port { get; private set; }
        public string Path { get; private set; }

        public MockSettings(string url, int port)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException($"'{nameof(url)}' cannot be null or empty.", nameof(url));
            
            Path = url;
            Port = port;
        }
    }
}
