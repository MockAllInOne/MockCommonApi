// See https://aka.ms/new-console-template for more information
using MockAllInOne.MockingModel.Parser;
using MockAllInOne.ServiceMock;

Console.WriteLine("Hello, World!");

var msgGen = new WsdlParser(@"E:\Data\OA_Server.xml");
msgGen.CreateModel();


var mock = new HttpMock(new MockSettings("MyApi", 8083));
mock.Start();

Console.ReadLine();

mock.Stop();

Console.WriteLine("Done.");