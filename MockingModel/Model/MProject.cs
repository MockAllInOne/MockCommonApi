namespace MockAllInOne.MockingModel.Model
{
    public class MProject
    {
        public string Name { get; }
        private List<MContainer> _containers;
        
        public MProject(string name)
        {
            Name = name;
            _containers = new List<MContainer>();
        }

        public void AddContainer(MContainer container) { _containers.Add(container); }
    }
}
