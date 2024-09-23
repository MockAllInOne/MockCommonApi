using MockAllInOne.MockingModel.Model.Interfaces;
using System.Xml.Serialization;

namespace MockAllInOne.MockingModel.Model
{
    public class MContainer : IMockContainer
    {
        [XmlElement("Id")]
        public string Id { get; private set; }

        private List<IMockEndpoint> _endpoints;

        public MContainer(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException($"'{nameof(id)}' cannot be null or empty.", nameof(id));

            Id = id;
            _endpoints = new List<IMockEndpoint>();
        }

        public void AddEndpoint(IMockEndpoint mOperation)
        {
            _endpoints.Add(mOperation);
        }

        public IReadOnlyCollection<IMockEndpoint> GetAllEndpoints()
        {
            return _endpoints.AsReadOnly();
        }
    }
}
