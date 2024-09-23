using MockAllInOne.MockingModel.MessageGenerator;
using MockAllInOne.MockingModel.Model;
using MockAllInOne.MockingModel.Model.Interfaces;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace MockAllInOne.MockingModel.Parser.Soap
{
    public class WsdlParser
    {
        private XDocument _wsdlDocument;
        private XNamespace _soapNamespace = "http://schemas.xmlsoap.org/wsdl/soap/";
        private XNamespace _soapNamespace12 = "http://schemas.xmlsoap.org/wsdl/soap12/";
        private XNamespace _wsdlNamespace = "http://schemas.xmlsoap.org/wsdl/";
        private XNamespace _xsdNamespace = "http://www.w3.org/2001/XMLSchema";
        private XmlNamespaceManager _namespaceManager;
        private XmlMessageGenerator _xsdToXmlGenerator;

        private readonly string input = "input";
        private readonly string output = "output";
        private readonly string fault = "fault";

        public WsdlParser(string wsdlPath)
        {
            if (string.IsNullOrWhiteSpace(wsdlPath))
                throw new ArgumentException($"'{nameof(wsdlPath)}' cannot be null or whitespace.", nameof(wsdlPath));

            _wsdlDocument = XDocument.Load(wsdlPath);

            _namespaceManager = new XmlNamespaceManager(new NameTable());
            _namespaceManager.AddNamespace("soap", _soapNamespace.NamespaceName);
            _namespaceManager.AddNamespace("soap12", _soapNamespace12.NamespaceName);
            _namespaceManager.AddNamespace("wsdl", _wsdlNamespace.NamespaceName);
            _namespaceManager.AddNamespace("xsd", _xsdNamespace.NamespaceName);

            _xsdToXmlGenerator = new XmlMessageGenerator(LoadSchemasFromWsdl());
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
                    PopulateProjectContainerFor(mContainer, portElement);
                }
            }

            return mProj;
        }

        private void PopulateProjectContainerFor(MContainer mContainer, XElement portElement)
        {
            string portBindingName = portElement.Attribute("binding")?.Value.Split(":").Last();
            string address = ((XElement)portElement.FirstNode).Attribute("location")?.Value;

            var endpoint = new MEndpoint(portBindingName);
            mContainer.AddEndpoint(endpoint);

            var operationElementsFromBinding = _wsdlDocument.XPathSelectElements($"//wsdl:binding[@name='{portBindingName}']/wsdl:operation", _namespaceManager);
            var bindingElem = _wsdlDocument.XPathSelectElement($"//wsdl:binding[@name='{portBindingName}']", _namespaceManager);
            var portName = bindingElem.Attribute("type")?.Value.Split(":").Last();
            var operationElementsFromPortType = _wsdlDocument.XPathSelectElements($"//wsdl:portType[@name='{portName}']/wsdl:operation", _namespaceManager);
            
            foreach (var operationElem in operationElementsFromBinding)
            {
                string operationName = operationElem.Attribute("name")?.Value;

                var operationFromPort = operationElementsFromPortType.FirstOrDefault(x => x.Attribute("name")?.Value == operationName);
                var soapActionValue = operationElem.Elements().FirstOrDefault(x => x.Name.LocalName == "operation")?.Attribute("soapAction")?.Value;

                var supportedMessages = GenerateSoapMessagesFor(operationElem, operationFromPort, soapActionValue);

                var mOperation = new MOperation(address, operationName, supportedMessages);
                endpoint.AddOperation(mOperation);
            }
        }

        private IReadOnlyCollection<IMockMessage> GenerateSoapMessagesFor(XElement operationElem, XElement operationFromPort, string soapActionValue)
        {
            return new List<IMockMessage>
            {
                CreateMockMessageFor(input, soapActionValue, MessageType.Request, operationElem, operationFromPort),
                CreateMockMessageFor(output, soapActionValue, MessageType.Response, operationElem, operationFromPort),
                CreateMockMessageFor(fault, soapActionValue, MessageType.Error, operationElem, operationFromPort)
            };
        }

        private IMockMessage CreateMockMessageFor(string messageType, string soapActionValue,
                            MessageType messageTypeLabel, XElement operationElem, XElement operationFromPort)
        {
            var httpHeader = new Dictionary<string, string>() { { "SOAPAction", soapActionValue } };

            SoapHeaderAndBodyType message = GetTypesFor(messageType, operationElem, operationFromPort);

            var header = _xsdToXmlGenerator.GenerateMockXmlMessage(message.HeaderType);
            var body = _xsdToXmlGenerator.GenerateMockXmlMessage(message.BodyType);

            return new MMessage(messageTypeLabel, httpHeader, GenerateSoapMessage(header, body).ToString());
        }

        private XDocument GenerateSoapMessage(string headerValue, string bodyValue, bool soapVersion12 = true)
        {
            XNamespace soapEnv = soapVersion12 ? _soapNamespace12 : _soapNamespace;

            var soapMessage = new XDocument(
                new XElement(soapEnv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapEnv),
                    new XElement(soapEnv + "Header", headerValue),
                    new XElement(soapEnv + "Body", bodyValue)
                )
            );

            return soapMessage;
        }

        private SoapHeaderAndBodyType GetTypesFor(string messageType, XElement operationElem, XElement operationFromPort)
        {
            string headerType = FindHeaderTypeInBinding(operationElem, messageType)
                                ?? FindHeaderTypeInPort(operationFromPort, messageType);

            string bodyType = FindBodyTypeInBinding(operationElem, messageType)
                              ?? FindBodyTypeInPort(operationFromPort, messageType);

            return new SoapHeaderAndBodyType(headerType, bodyType);
        }

        private string? FindHeaderTypeInBinding(XElement operationElem, string messageType)
        {
            XElement messageElem = operationElem.Elements().FirstOrDefault(x => x.Name.LocalName == messageType);

            if (messageElem == null)
                return null;

            var headerTag = messageElem.Elements().FirstOrDefault(x => x.Name.LocalName == "header");
            
            return headerTag?.Attribute("part")?.Value;
        }

        private string? FindHeaderTypeInPort(XElement operationFromPort, string messageType)
        {
            XElement messageElem = operationFromPort.Elements().FirstOrDefault(x => x.Name.LocalName == messageType);
            var msgName = messageElem?.Attribute("message")?.Value;
            
            if (msgName == null)
                return null;

            var msgtag = _wsdlDocument.XPathSelectElement($"//wsdl:message[@name='{msgName}']", _namespaceManager);
            return msgtag?.Attribute("element")?.Value.Split(":").Last();
        }

        private string? FindBodyTypeInBinding(XElement operationElem, string messageType)
        {
            XElement messageElem = operationElem.Elements().FirstOrDefault(x => x.Name.LocalName == messageType);

            if (messageElem == null)
                return null;

            var bodyTag = messageElem.Elements().FirstOrDefault(x => x.Name.LocalName == "body");

            var msgName = bodyTag?.Attribute("message")?.Value;
            if(msgName == null)
                return null;

            var msgtag = _wsdlDocument.XPathSelectElement($"//wsdl:message[@name='{msgName}']", _namespaceManager);
            return msgtag?.Attribute("element")?.Value.Split(":").Last();
        }

        private string? FindBodyTypeInPort(XElement operationFromPort, string messageType)
        {
            XElement messageElem = operationFromPort.Elements().FirstOrDefault(x => x.Name.LocalName == messageType);

            if (messageElem == null)
                return null;

            var msgName = messageElem?.Attribute("message")?.Value.Split(":").Last();
            if (msgName == null)
                return null;

            var msgtag = _wsdlDocument.XPathSelectElement($"//wsdl:message[@name='{msgName}']", _namespaceManager);
            
            return msgtag?.Elements().FirstOrDefault(x => x.Name.LocalName == "part")?.Attribute("element")?.Value.Split(":").Last();
        }

        private XmlSchemaSet LoadSchemasFromWsdl()
        {
            var schemaSet = new XmlSchemaSet();
            var schemas = _wsdlDocument.XPathSelectElements("//wsdl:types/xsd:schema", _namespaceManager);
            foreach (var schemaElement in schemas)
            {
                using (var reader = schemaElement.CreateReader())
                {
                    schemaSet.Add(XmlSchema.Read(reader, null));
                }
            }
            schemaSet.Compile();
            return schemaSet;
        }
    }

    internal record struct SoapHeaderAndBodyType(string? HeaderType, string? BodyType)
    {
        public static implicit operator (string? HeaderType, string? BodyType)(SoapHeaderAndBodyType value)
        {
            return (value.HeaderType, value.BodyType);
        }

        public static implicit operator SoapHeaderAndBodyType((string? HeaderType, string? BodyType) value)
        {
            return new SoapHeaderAndBodyType(value.HeaderType, value.BodyType);
        }
    }
}
