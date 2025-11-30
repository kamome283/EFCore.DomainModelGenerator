using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.GenerationStep;

using static Common;

internal static class CollectSetMetadata
{
  public const string TargetAttribute = "DomainSetAttribute";

  public static SetMetadata Collect(GeneratorAttributeSyntaxContext source, CancellationToken _)
  {
    var symbol = source.TargetSymbol as IPropertySymbol ?? throw new CollectSetMetadataException("symbol");
    var propType = symbol.Type as INamedTypeSymbol;
    if (propType is null or not { IsGenericType: true, ConstructedFrom.Name: "DbSet" })
      throw new CollectSetMetadataException("propType");
    var attr = symbol.GetAttributesOf($"{GeneratorNamespace}.{TargetAttribute}").SingleOrDefault()
               ?? throw new CollectSetMetadataException("attr");
    return new SetMetadata
    {
      ParentType = symbol.ContainingType as ITypeSymbol ??
                   throw new CollectSetMetadataException("ParentType"),
      DomainName = attr.GetArgumentAt<string>(0) ?? symbol.Name,
      MappedName = attr.GetArgumentAt<string>(1) ?? symbol.Name,
      OriginalName = symbol.Name,
      ElementType = propType.TypeArguments.Single(),
      ReadonlyAccessibility = GetAccessibility(attr.GetArgumentAt(2) as int?)
                              ?? throw new CollectSetMetadataException("ReadonlyAccessibility"),
      WritableAccessibility = GetAccessibility(attr.GetArgumentAt(3) as int?)
                              ?? throw new CollectSetMetadataException("WritableAccessibility"),
    };
  }

  private static string GetAccessibility(int? accessibility)
  {
    return accessibility switch
    {
      0 => "public",
      1 => "internal",
      2 => "protected",
      _ => throw new CollectSetMetadataException("accessibility"),
    };
  }
}

internal record SetMetadata
{
  public ITypeSymbol ParentType { get; set; } = null!;
  public string DomainName { get; set; } = null!;
  public string MappedName { get; set; } = null!;
  public string OriginalName { get; set; } = null!;
  public ITypeSymbol ElementType { get; set; } = null!;
  public string ReadonlyAccessibility { get; set; } = null!;
  public string WritableAccessibility { get; set; } = null!;

  public string ModelName => $"{DomainName}Domain";
}

file class CollectSetMetadataException(string segment) : Exception(segment);
