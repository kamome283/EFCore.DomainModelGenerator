using Microsoft.EntityFrameworkCore;
using SampleProgram.Entities;
using SampleProgram.Repository;

namespace SampleProgram;

internal static class SeedingHelper
{
  public static async Task SeedDataAsync(DbContext boxedDb, bool _, CancellationToken ct)
  {
    var db = (PrimaryDb)boxedDb;
    var staffs = new[]
    {
      new Staff
      {
        Name = "John",
        Details = new StaffDetails { Address = "Naniwa Ward, Osaka", Email = "John@example.com" },
      },
      new Staff
      {
        Name = "Liz",
        Details = new StaffDetails { Address = null, Email = "Andress@example.com" },
      },
    };
    await db.Staffs.AddRangeAsync(staffs, ct);

    var john = staffs[0];
    var date = new DateTime(2025, 11, 24);
    var schedule = new StaffSchedule() { Staff = john, Start = date.AddHours(9), End = date.AddHours(18) };
    await db.StaffSchedules.AddAsync(schedule, ct);

    var customers = new[]
    {
      new Customer { Name = "Alice" },
      new Customer { Name = "Bob" },
    };
    await db.Customers.AddRangeAsync(customers, ct);

    await db.SaveChangesAsync(ct);
  }
}
