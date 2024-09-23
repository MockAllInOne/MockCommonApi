using MockAllInOne.MockingModel.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MockAllInOne.MockingModel.Model
{
    public class MEndpoint : IMockEndpoint
    {
        private List<IMockOperation> _operations;

        [XmlElement("Id")]
        public string Id { get; private set; }

        public MEndpoint(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException($"'{nameof(id)}' cannot be null or empty.", nameof(id));

            Id = id;
            _operations = new List<IMockOperation>();
        }

        public void AddOperation(IMockOperation mOperation)
        {
            _operations.Add(mOperation);
        }

        public IReadOnlyCollection<IMockOperation> GetAllOperations()
        {
            return _operations.AsReadOnly();
        }
    }
}
