using System.ComponentModel.DataAnnotations;

namespace SampleProgram.Entities;

public class Customer
{
  public Guid Id { get; init; }
  [Required, MaxLength(20)] public required string Name { get; init; }
}
