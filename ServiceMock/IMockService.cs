namespace MockAllInOne.ServiceMock
{
    public interface IMockService
    {
        bool IsStarted { get; }
        void Start();
        void Stop();
    }
}
