using SampleWorker;

var a = new SampleClass();
a.SayHello();
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
