using EFCore.DomainModelGenerator;
using SampleWorker;

var a = new SampleClass();
a.SayHello();
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

[DomainContext("SampleWorker.Domains")]
file class SampleContext
{
  [DomainSet] internal object[] Guests { get; set; }
  [DomainSet(nameof(Guests))] internal object[] GuestComments { get; set; }
  [DomainSet(nameof(Guests), isPrivate: true)] internal object[] GuestProfiles { get; set; }
}
