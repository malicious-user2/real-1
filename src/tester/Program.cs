// See https://aka.ms/new-console-template for more information
using Google.Protobuf;

Console.WriteLine("Hello, World!");

var client_secrets = new YouRatta.Common.Proto.ClientSecrets();
var installed_client_secrets = new YouRatta.Common.Proto.InstalledClientSecrets();
installed_client_secrets.ClientId = "123";
client_secrets.InstalledClientSecrets = installed_client_secrets;
Console.WriteLine(JsonFormatter.Default.Format(client_secrets));
