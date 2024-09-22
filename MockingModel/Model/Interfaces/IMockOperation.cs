namespace MockAllInOne.MockingModel.Model.Interfaces
{
    public interface IMockOperation
    {
        string Address { get; }
        string OperationName { get; }

        string GenerateMessage(string messageId);
    }
}