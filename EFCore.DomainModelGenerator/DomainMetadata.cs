using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator;

internal record DomainMetadata
{
  public string DomainClassName { get; set; } = null!;
  public IEnumerable<DomainSetMetadata> DomainSetMetadata { get; set; } = null!;
}

internal record DomainSetMetadata
{
  public string DomainName { get; set; } = null!;
  public string MappedName { get; set; } = null!;
  public string OriginalName { get; set; } = null!;
  public ITypeSymbol ElementType { get; set; } = null!;
  public bool IsPrivate { get; set; }
}
