using System.ComponentModel.DataAnnotations;

namespace SampleProgram.Entities;

public class StaffDetails
{
  public Guid Id { get; init; }
  public Guid StaffId { get; init; }
  public Staff Staff { get; init; } = null!;
  [MaxLength(256)] public required string? Address { get; init; }
  [Required, EmailAddress, MaxLength(254)] public required string Email { get; init; }
}
