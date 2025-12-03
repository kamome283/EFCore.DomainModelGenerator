using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.Steps;

using static Common;

internal static class CollectSets
{
  public const string TargetAttribute = "DomainSetAttribute";

  public static SetMetadata Collect(GeneratorAttributeSyntaxContext source, CancellationToken _)
  {
    var symbol = source.TargetSymbol as IPropertySymbol ?? throw new CollectSetsException("symbol");
    var propType = symbol.Type as INamedTypeSymbol;
    if (propType is null or not { IsGenericType: true, ConstructedFrom.Name: "DbSet" })
      throw new CollectSetsException("propType");
    var attr = symbol.GetAttributesOf($"{GeneratorNamespace}.{TargetAttribute}").SingleOrDefault()
               ?? throw new CollectSetsException("attr");
    return new SetMetadata
    {
      ParentType = symbol.ContainingType as ITypeSymbol ??
                   throw new CollectSetsException("ParentType"),
      DomainName = attr.GetArgumentAt(0) as string ?? symbol.Name,
      MappedName = attr.GetArgumentAt(1) as string ?? symbol.Name,
      OriginalName = symbol.Name,
      ElementType = propType.TypeArguments.Single(),
      ReadonlyAccessibility = GetAccessibility(attr.GetArgumentAt(2) as int?)
                              ?? throw new CollectSetsException("ReadonlyAccessibility"),
      WritableAccessibility = GetAccessibility(attr.GetArgumentAt(3) as int?)
                              ?? throw new CollectSetsException("WritableAccessibility"),
    };
  }

  private static string GetAccessibility(int? accessibility)
  {
    return accessibility switch
    {
      0 => "public",
      1 => "internal",
      2 => "protected",
      _ => throw new CollectSetsException("accessibility"),
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

internal class CollectSetsException(string segment) : InvalidOperationException(segment);
