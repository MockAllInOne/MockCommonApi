using MockAllInOne.MockingModel.Model.Interfaces;

namespace MockAllInOne.MockingModel.Model
{
    public class MContainer : IMockContainer
    {
        public string Id { get; private set; }
        private List<IMockOperation> _operations;

        public MContainer(string id)
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
    }
}
