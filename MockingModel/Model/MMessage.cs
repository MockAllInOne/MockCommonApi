using MockAllInOne.MockingModel.Model.Interfaces;

namespace MockAllInOne.MockingModel.Model
{
    public enum MessageType 
    {
        Request,
        Response,
        Error,
        Unknown
    }

    public class MMessage : IMockMessage
    {
        public MessageType Type { get; }
        public Dictionary<string, string> Headers { get; }
        public string Message { get; }

        public MMessage(MessageType type, Dictionary<string, string> headers, string message)
        {
            Type = type;
            Headers = headers;
            Message = message;
        }
    }
}
