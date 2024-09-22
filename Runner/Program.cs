using MockAllInOne.MockingModel.Parser;
using MockAllInOne.ServiceMock;


var msgGen = new WsdlParser(@"E:\Data\RBA_Server.xml");
msgGen.CreateModel();


var mock = new HttpMock(new MockSettings("MyApi", 8088));
mock.Start();

Console.ReadLine();
mock.Stop();
Console.WriteLine("Done.");