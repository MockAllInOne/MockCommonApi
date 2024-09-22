using System.Xml.Serialization;

namespace MockAllInOne.MockingModel.Model
{
    [XmlRoot("Project")]
    public class MProject
    {
        [XmlElement("Name")]
        public string Name { get; }

        private List<MContainer> _containers;
        
        public MProject(string name)
        {
            Name = name;
            _containers = new List<MContainer>();
        }

        public void AddContainer(MContainer container) { _containers.Add(container); }

        public List<MContainer> GetAllContainers()
        {
            return _containers;
        }
    }
}
