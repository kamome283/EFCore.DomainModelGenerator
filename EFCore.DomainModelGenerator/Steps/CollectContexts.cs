using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.Steps;

using static Common;

internal static class CollectContexts
{
  public const string TargetAttribute = "DomainContextAttribute";

  public static ContextMetadata Collect(GeneratorAttributeSyntaxContext source, CancellationToken _)
  {
    var symbol = source.TargetSymbol as INamedTypeSymbol ?? throw new CollectContextsException("symbol");
    var attr = symbol.GetAttributesOf($"{GeneratorNamespace}.{TargetAttribute}").SingleOrDefault()
               ?? throw new CollectContextsException("attr");
    // throw new CollectContextMetadataException(attr.ToString());
    return new ContextMetadata
    {
      Namespace = attr.GetArgumentAt(0) as string ?? throw new CollectContextsException("Namespace"),
      ContextType = symbol,
    };
  }
}

internal record ContextMetadata
{
  public string Namespace { get; set; } = null!;
  public INamedTypeSymbol ContextType { get; set; } = null!;
}

internal class CollectContextsException(string segment) : InvalidOperationException(segment);
