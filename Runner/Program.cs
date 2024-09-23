using MockAllInOne.MockingModel.Parser.Soap;
using MockAllInOne.ServiceMock;

var parser1 = new WsdlParser(@"E:\Data\QC_Server.xml");
var proj1 = parser1.CreateNewProject("P1");

var parser2 = new WsdlParser(@"https://www.xignite.com/xCurrencies.asmx?wsdl");
var proj = parser2.CreateNewProject("Proj2");


var mock = new MockService(new MockSettings("MyApi", 8088));
mock.Start();

Console.ReadLine();
mock.Stop();
Console.WriteLine("Done."); 