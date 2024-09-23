namespace MockAllInOne.MockingModel.Model.Interfaces
{
    public interface IMockOperation
    {
        string Address { get; }
        string OperationName { get; }
        
        IReadOnlyCollection<IMockMessage> GetSupportedMessages();
    }
}