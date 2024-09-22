namespace MockAllInOne.MockingModel.Model
{
    public class MContainer
    {
        public string Id { get; private set; }
        private List<MOperation> _operations;

        public MContainer(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException($"'{nameof(id)}' cannot be null or empty.", nameof(id));

            Id = id;
            _operations = new List<MOperation>();
        }

        public void AddOperation(MOperation mOperation)
        {
            _operations.Add(mOperation);
        }
    }
}
