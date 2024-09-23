using System.Xml.Linq;
using System.Xml.Schema;

namespace MockAllInOne.MockingModel.MessageGenerator
{
    public class XmlMessageGenerator
    {
        private XmlSchemaSet _schemaSet;

        public XmlMessageGenerator(XmlSchemaSet schemaSet)
        {
            _schemaSet = schemaSet ?? throw new ArgumentNullException(nameof(schemaSet));
            if (!schemaSet.IsCompiled)
                schemaSet.Compile();
        }

        public string? GenerateMockXmlMessage(string elementType)
        {
            if (elementType == null)
                return null;

            XmlSchemaElement schemaElement = FindSchemaElement(elementType);
            if (schemaElement == null)
                throw new Exception($"Element '{elementType}' not found in schema..");

            XNamespace ns = schemaElement.QualifiedName.Namespace;

            var xmlDoc = new XDocument();
            var rootElement = new XElement(ns + schemaElement.QualifiedName.Name);
            xmlDoc.Add(rootElement);

            PopulateElement(schemaElement, rootElement, ns);

            return xmlDoc.ToString();
        }

        private XmlSchemaElement FindSchemaElement(string elementName)
        {
            foreach (XmlSchema schema in _schemaSet.Schemas())
            {
                var element = schema.Elements.Values.OfType<XmlSchemaElement>().FirstOrDefault(e => e.Name == elementName);
                if (element != null)
                {
                    return element;
                }
            }
            return null;
        }

        private void PopulateElement(XmlSchemaElement schemaElement, XElement xmlElement, XNamespace ns)
        {
            string commentText = GenerateComment(schemaElement);
            xmlElement.AddBeforeSelf(new XComment(commentText));

            if (schemaElement.ElementSchemaType is XmlSchemaComplexType complexType)
            {
                if (complexType.AttributeUses.Count > 0)
                {
                    foreach (XmlSchemaAttribute attribute in complexType.AttributeUses.Values)
                    {
                        if (attribute == null || attribute.AttributeSchemaType == null)
                            continue;

                        string attrValue = GenerateDummyData(attribute.AttributeSchemaType.TypeCode);
                        XNamespace attrNamespace = attribute.QualifiedName.Namespace;
                        xmlElement.SetAttributeValue(attrNamespace + attribute.Name, attrValue);
                    }
                }

                if (complexType.ContentTypeParticle is XmlSchemaSequence sequence)
                {
                    foreach (XmlSchemaElement childElement in sequence.Items.OfType<XmlSchemaElement>())
                    {
                        XNamespace childNs = childElement.QualifiedName.Namespace;
                        XElement childXmlElement = new XElement(childNs + childElement.Name);
                        xmlElement.Add(childXmlElement);
                        PopulateElement(childElement, childXmlElement, childNs);
                    }
                }
            }
            else
            {
                xmlElement.Value = GenerateDummyData(schemaElement.ElementSchemaType.TypeCode);
            }
        }

        private string GenerateComment(XmlSchemaElement schemaElement)
        {
            decimal minOccurs = schemaElement.MinOccurs;
            decimal maxOccurs = schemaElement.MaxOccurs;

            if (minOccurs == 0 && maxOccurs == 1)
                return "Optional";

            if (minOccurs == 1 && maxOccurs == 1)
                return "Mandatory";

            if (minOccurs == 0 && maxOccurs > 1)
                return "None or many";

            if (minOccurs == 1 && maxOccurs > 1)
                return "One or many";

            return string.Empty;
        }

        private string GenerateDummyData(XmlTypeCode typeCode)
        {
            return typeCode switch
            {
                XmlTypeCode.String => "string",
                XmlTypeCode.Boolean => (new Random().Next(2) == 0).ToString(),
                XmlTypeCode.Decimal => "12.44",
                XmlTypeCode.Float => new Random().Next(1, 1000).ToString(),
                XmlTypeCode.Double => new Random().Next(1, 1000).ToString(),
                XmlTypeCode.Duration => new Random().Next(1, 1000).ToString(),
                XmlTypeCode.DateTime => DateTime.Now.ToString("o"),
                XmlTypeCode.Time => DateTime.Now.TimeOfDay.ToString(),
                XmlTypeCode.Date => DateTime.Now.Date.ToString(),
                XmlTypeCode.AnyUri => "http://www.cra.zy/",
                XmlTypeCode.Integer => new Random().Next(1, 1000).ToString(),
                XmlTypeCode.Long => new Random().Next(1, 1000).ToString(),
                XmlTypeCode.Int => new Random().Next(1, 1000).ToString(),
                XmlTypeCode.Short => new Random().Next(1, 1000).ToString(),
                XmlTypeCode.Byte => "1",
                XmlTypeCode.UnsignedShort => "1",
                XmlTypeCode.UnsignedByte => "1",
                XmlTypeCode.PositiveInteger => new Random().Next(1, 1000).ToString(),
                _ => "dummyValue",
            };
        }
    }

}
