using System.ComponentModel.DataAnnotations;

namespace SampleProgram.Entities;

public class Staff
{
  public Guid Id { get; init; }
  [Required, MaxLength(20)] public required string Name { get; init; }

  public ICollection<StaffSchedule> Schedules { get; } = new List<StaffSchedule>();
  public required StaffDetails Details { get; init; }
}
