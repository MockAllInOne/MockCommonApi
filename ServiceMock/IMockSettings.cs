namespace MockAllInOne.ServiceMock
{
    public interface IMockSettings
    {
        /// <summary>
        /// Mock port number
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Url path name
        /// </summary>
        public string Path { get; }

    }
}