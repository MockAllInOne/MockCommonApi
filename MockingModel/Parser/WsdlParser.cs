using MockAllInOne.MockingModel.MessageGenerator.Soap;
using MockAllInOne.MockingModel.Model;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace MockAllInOne.MockingModel.Parser
{
    public class WsdlParser
    {
        private XDocument _wsdlDocument;
        private XmlSchemaSet _schemaSet;
        private XNamespace _soapNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
        private XNamespace _wsdlNamespace = "http://schemas.xmlsoap.org/wsdl/";
        private XNamespace _xsdNamespace = "http://www.w3.org/2001/XMLSchema";
        private XmlNamespaceManager _namespaceManager;

        public WsdlParser(string wsdlPath)
        {
            if (string.IsNullOrWhiteSpace(wsdlPath))
                throw new ArgumentException($"'{nameof(wsdlPath)}' cannot be null or whitespace.", nameof(wsdlPath));

            _wsdlDocument = XDocument.Load(wsdlPath);

            _namespaceManager = new XmlNamespaceManager(new NameTable());
            _namespaceManager.AddNamespace("soap", _soapNamespace.NamespaceName);
            _namespaceManager.AddNamespace("wsdl", _wsdlNamespace.NamespaceName);
            _namespaceManager.AddNamespace("xsd", _xsdNamespace.NamespaceName);
            LoadSchemasFromWsdl();
        }

        public MProject CreateNewProject(string projectName)
        {
            var mProj = new MProject(projectName);
            var serviceElements = _wsdlDocument.XPathSelectElements("//wsdl:service", _namespaceManager);
            
            foreach (var serviceElement in serviceElements)
            {
                var serviceName = serviceElement.Attribute("name")?.Value;
                var mContainer = new MContainer(serviceName);
                mProj.AddContainer(mContainer);

                var portElements = serviceElement.XPathSelectElements("//wsdl:port", _namespaceManager);
                foreach (var portElement in portElements)
                {
                    string portName = portElement.Attribute("name")?.Value;
                    string portBinding = portElement.Attribute("binding")?.Value;

                    string address = ((XElement)portElement.FirstNode).Attribute("location")?.Value;

                    var operationElements = _wsdlDocument.XPathSelectElements($"//wsdl:portType[@name='{portName}']/wsdl:operation", _namespaceManager);

                    foreach (var operationElement in operationElements)
                    {
                        string operationName = operationElement.Attribute("name")?.Value;

                        // Get the message structure and generate the XML for it
                        List<string> messageTypeNames = GetMessagesTypeNames(operationElement);
                        var mOperation = new MOperation(address, operationName, messageTypeNames, new XmlMessageGenerator(_schemaSet));//TODO: msg gen
                        mContainer.AddOperation(mOperation);
                    }
                }
            }
            return mProj;
        }

        private List<string> GetMessagesTypeNames(XElement operationElement)
        {
            var msgTypes = new List<string>();
            var operationElements = operationElement.Nodes().OfType<XElement>();

            var inputElement = operationElements.FirstOrDefault(x => x.Name.LocalName == "input");
            AddMsgTypeNameTo(msgTypes, inputElement);
            var outputElement = operationElements.FirstOrDefault(x => x.Name.LocalName == "output");
            AddMsgTypeNameTo(msgTypes, outputElement);
            var faultElement = operationElements.FirstOrDefault(x => x.Name.LocalName == "fault");
            AddMsgTypeNameTo(msgTypes, faultElement);

            return msgTypes;
        }

        private void AddMsgTypeNameTo(List<string> msgTypes, XElement inputElement)
        {
            if (inputElement == null)
                return;

            string msgType = inputElement.Attribute("message")?.Value;
            if (msgType == null)
                return;

            string messageName = msgType.Split(":").Last();
            var messageElement = _wsdlDocument.XPathSelectElement($"//wsdl:message[@name='{messageName}']", _namespaceManager);
            if (messageElement == null)
                return;

            string typeName = (messageElement.FirstNode as XElement)?.Attribute("element")?.Value;
            if (typeName == null)
                return;

            msgTypes.Add(typeName.Split(":").Last());
        }

        private void LoadSchemasFromWsdl()
        {
            _schemaSet = new XmlSchemaSet();
            var schemas = _wsdlDocument.XPathSelectElements("//wsdl:types/xsd:schema", _namespaceManager);
            foreach (var schemaElement in schemas)
            {
                using (var reader = schemaElement.CreateReader())
                {
                    _schemaSet.Add(XmlSchema.Read(reader, null));
                }
            }
            _schemaSet.Compile();
        }
    }
}
