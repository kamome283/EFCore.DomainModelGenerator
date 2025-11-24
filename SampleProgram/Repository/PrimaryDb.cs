using Microsoft.EntityFrameworkCore;
using SampleProgram.Entities;

namespace SampleProgram.Repository;

public class PrimaryDb(DbContextOptions<PrimaryDb> options) : DbContext(options)
{
  internal DbSet<Staff> Staffs { get; init; }
  internal DbSet<StaffSchedule> StaffSchedules { get; init; }
  internal DbSet<StaffDetails> StaffDetails { get; init; }

  internal DbSet<Customer> Customers { get; init; }
}
