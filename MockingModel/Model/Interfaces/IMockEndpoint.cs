namespace MockAllInOne.MockingModel.Model.Interfaces
{
    public interface IMockEndpoint
    {
        IReadOnlyCollection<IMockOperation> GetAllOperations();
    }
}
