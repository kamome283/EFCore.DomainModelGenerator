using EFCore.DomainModelGenerator.AnalysisResult;
using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.Steps;

internal static class CollectContexts
{
  public const string TargetAttribute = "DomainContextAttribute";

  public static AnalysisResult<ContextMetadata> Collect(GeneratorAttributeSyntaxContext source, CancellationToken _)
  {
    var symbol = source.TargetSymbol as INamedTypeSymbol ?? throw new CollectContextsException("symbol");
    return new AnalysisResult<ContextMetadata>
    {
      Result = new ContextMetadata { ContextType = symbol },
    };
  }
}

internal record ContextMetadata
{
  public INamedTypeSymbol ContextType { get; set; } = null!;
}

internal class CollectContextsException(string segment) : InvalidOperationException(segment);
