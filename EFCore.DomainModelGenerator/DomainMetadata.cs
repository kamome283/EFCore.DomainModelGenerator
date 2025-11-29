using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator;

internal record DomainMetadata
{
  public string DomainName { get; set; } = null!;
  public IEnumerable<DomainSetMetadata> DomainSetMetadata { get; set; } = null!;

  public string ReadonlyDomainClass => $"{DomainName}Domain";
  public string WritableDomainClass => $"Writable{DomainName}Domain";
}

internal record DomainSetMetadata
{
  public string DomainName { get; set; } = null!;
  public string MappedName { get; set; } = null!;
  public string OriginalName { get; set; } = null!;
  public ITypeSymbol ElementType { get; set; } = null!;
  public string ReadonlyAccessibility { get; set; } = null!;
  public string WritableAccessibility { get; set; } = null!;
}
