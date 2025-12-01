using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.GenerationStep;

using static Common;

internal static class CollectContextMetadata
{
  public const string TargetAttribute = "DomainContextAttribute";

  public static ContextMetadata Collect(GeneratorAttributeSyntaxContext source, CancellationToken _)
  {
    var symbol = source.TargetSymbol as INamedTypeSymbol ?? throw new CollectContextMetadataException("symbol");
    var attr = symbol.GetAttributesOf($"{GeneratorNamespace}.{TargetAttribute}").SingleOrDefault()
               ?? throw new CollectContextMetadataException("attr");
    // throw new CollectContextMetadataException(attr.ToString());
    return new ContextMetadata
    {
      Namespace = attr.GetArgumentAt(0) as string ?? throw new CollectContextMetadataException("Namespace"),
      ContextType = symbol,
    };
  }
}

internal record ContextMetadata
{
  public string Namespace { get; set; } = null!;
  public INamedTypeSymbol ContextType { get; set; } = null!;
}

internal class CollectContextMetadataException(string segment) : InvalidOperationException(segment);
