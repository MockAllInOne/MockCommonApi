namespace MockAllInOne.MockingModel.Model.Interfaces
{
    public interface IMockContainer
    {
        string Id { get; }

        IReadOnlyCollection<IMockEndpoint> GetAllEndpoints();
    }
}
