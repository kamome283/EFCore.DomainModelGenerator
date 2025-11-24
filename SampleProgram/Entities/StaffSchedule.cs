namespace SampleProgram.Entities;

public class StaffSchedule
{
  public Guid Id { get; init; }
  public required Staff Staff { get; init; }
  public required DateTime Start { get; init; }
  public required DateTime End { get; init; }
}
