using MockAllInOne.MockingModel.Parser;
using MockAllInOne.ServiceMock;


var msgGen = new WsdlParser(@"https://www.xignite.com/xCurrencies.asmx?wsdl");
var proj = msgGen.CreateNewProject("Proj1");

var someOperations = proj.GetAllContainers().First().GetAllOperations();

foreach (var operation in someOperations)
{
    var msgIds = operation.GetSupportedMessages();
    foreach (var msgId in msgIds)
    {
        var soapMockMessage = operation.GenerateMessage(msgId);
        Console.WriteLine($"SoapMessage: {soapMockMessage}.\n"); 
    }
}


var mock = new MockService(new MockSettings("MyApi", 8088));
mock.Start();

Console.ReadLine();
mock.Stop();
Console.WriteLine("Done.");