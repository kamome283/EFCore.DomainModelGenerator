using Microsoft.EntityFrameworkCore;
using SampleProgram;
using SampleProgram.Domains.Customers;
using SampleProgram.Domains.DomainRegistration;
using SampleProgram.Domains.Staffs;
using SampleProgram.Repository;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
  .AddDbContext<PrimaryDb>(options =>
  {
    options.UseSqlite("Data Source=test.db");
    options.UseAsyncSeeding(SeedingHelper.SeedDataAsync);
  })
  .AddDomains()
  .AddScoped<WorkerImpl>()
  .AddHostedService<WorkerWrapper>();
var host = builder.Build();
host.Run();

file class WorkerImpl(
  ILogger<WorkerImpl> logger,
  PrimaryDb db,
  StaffsDomain staffsDomain,
  CustomersDomain customersDomain
)
{
  public async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await db.Database.MigrateAsync(stoppingToken);

    var date = new DateTime(2025, 11, 24);
    var staffsOnDate = staffsDomain.GetStaffsOnDateAsync(date).WithCancellation(stoppingToken);
    await foreach (var staff in staffsOnDate)
    {
      logger.LogInformation("Staff on {date}: {staff}({email})", date, staff.Name, staff.Details.Email);
    }

    var specialCustomers = customersDomain
      .Customers
      .Where(x => x.Name.StartsWith('A'))
      .ToAsyncEnumerable()
      .WithCancellation(stoppingToken);
    await foreach (var customer in specialCustomers)
    {
      logger.LogInformation("Special customer: {customer}", customer.Name);
    }
  }
}

// DbContext is scoped service, thus dependent services are also scoped,
// therefore worker implementation has to be run on some scope.
file class WorkerWrapper(IServiceScopeFactory scopeFactory, IHostApplicationLifetime lifetime) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    using var scope = scopeFactory.CreateScope();
    var impl = scope.ServiceProvider.GetRequiredService<WorkerImpl>();
    await impl.ExecuteAsync(lifetime.ApplicationStopping);
    lifetime.StopApplication();
  }
}
