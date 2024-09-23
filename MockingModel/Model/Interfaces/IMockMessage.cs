namespace MockAllInOne.MockingModel.Model.Interfaces
{
    public interface IMockMessage
    {
        MessageType Type { get; }
        Dictionary<string, string> Headers { get; }
        string Message { get; }
    }
}
