namespace SampleProgram.Domains;

// This helper class should be auto generated
public static class DomainRegistrationHelper
{
  public static IServiceCollection AddDomains(this IServiceCollection services)
  {
    services.AddScoped<StaffsDomain>();
    services.AddScoped<CustomersDomain>();
    return services;
  }
}
