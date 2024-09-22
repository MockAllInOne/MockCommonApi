using MockAllInOne.MockingModel.MessageGenerator.Soap;
using MockAllInOne.MockingModel.Model.Interfaces;

namespace MockAllInOne.MockingModel.Model
{
    public class MOperation : IMockOperation
    {
        public string Address { get; }
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
