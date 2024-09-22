using MockAllInOne.MockingModel.MessageGenerator.Soap;
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

        private XmlMessageGenerator _xmlMessageGenerator;
        private IReadOnlyCollection<string> _messageTypeNames;

        public MOperation(string defaultAddress, string operationName, List<string> messageTypeNames, XmlMessageGenerator xmlMessageGenerator)
        {
            Address = defaultAddress;
            OperationName = operationName;
            _messageTypeNames = messageTypeNames;
            _xmlMessageGenerator = xmlMessageGenerator;
        }

        public IReadOnlyCollection<string> GetSupportedMessages() 
        {
            return _messageTypeNames;
        }

        public string GenerateMessage(string supportedMessage) 
        {
            return _xmlMessageGenerator.GenerateMockXmlMessage(supportedMessage);
        }
    }
}
