using MockAllInOne.MockingModel.Model.Interfaces;
using System.Xml.Serialization;

namespace MockAllInOne.MockingModel.Model
{
    public class MOperation : IMockOperation
    {
        [XmlElement("Address")]
        public string Address { get; }

        [XmlElement("OperationName")]
        public string OperationName { get; }

        private IReadOnlyCollection<IMockMessage> _supportedMessages;

        public MOperation(string defaultAddress, string operationName, IReadOnlyCollection<IMockMessage> supportedMessages)
        {
            Address = defaultAddress;
            OperationName = operationName;
            _supportedMessages = supportedMessages;
        }

        public IReadOnlyCollection<IMockMessage> GetSupportedMessages()
        {
            return _supportedMessages;
        }

    }
}
