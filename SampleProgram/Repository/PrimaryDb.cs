using EFCore.DomainModelGenerator;
using Microsoft.EntityFrameworkCore;
using SampleProgram.Entities;

namespace SampleProgram.Repository;

[DomainContext("SampleProgram.Domains")]
public class PrimaryDb(DbContextOptions<PrimaryDb> options) : DbContext(options)
{
  [DomainSet] internal DbSet<Staff> Staffs { get; init; }
  [DomainSet(nameof(Staffs), "Schedules")] internal DbSet<StaffSchedule> StaffSchedules { get; init; }
  [DomainSet(nameof(Staffs), isPrivate: true)] internal DbSet<StaffDetails> StaffDetails { get; init; }

  [DomainSet] internal DbSet<Customer> Customers { get; init; }
}
