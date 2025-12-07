using EFCore.DomainModelGenerator;
using Microsoft.EntityFrameworkCore;
using SampleProgram.Entities;

namespace SampleProgram.Repository;

[DomainContext]
public class PrimaryDb(DbContextOptions<PrimaryDb> options) : DbContext(options)
{
  [DomainSet] internal DbSet<Staff> Staffs { get; init; }
  [DomainSet(nameof(Staffs), "Schedules")] internal DbSet<StaffSchedule> StaffSchedules { get; init; }

  [DomainSet(nameof(Staffs), readonlyDomain: Access.Protected, writableDomain: Access.Internal)]
  internal DbSet<StaffDetails> StaffDetails { get; init; }

  [DomainSet] internal DbSet<Customer> Customers { get; init; }
}

// // If DomainSet is used outside DomainContext, a diagnostic error will be issued.
//
// public class NonDomainSetContext(DbContextOptions<NonDomainSetContext> options) : DbContext(options)
// {
//   [DomainSet] internal DbSet<Staff> SecondaryStaffs { get; init; }
// }
