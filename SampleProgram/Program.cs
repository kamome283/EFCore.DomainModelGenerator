using EFCore.DomainModelGenerator;
using SampleProgram;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

[DomainContext("SampleWorker.Domains")]
file class SampleContext
{
  [DomainSet] internal object[] Guests { get; set; } = null!;
  [DomainSet(nameof(Guests))] internal object[] GuestComments { get; set; } = null!;
  [DomainSet(nameof(Guests), isPrivate: true)] internal object[] GuestProfiles { get; set; } = null!;
}
