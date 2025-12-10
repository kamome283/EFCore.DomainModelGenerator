using EFCore.DomainModelGenerator.AnalysisResult;
using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.Steps;

using static Common;

internal static class CollectModels
{
  public const string TargetAttribute = "DomainModelAttribute";
  private const string DependsOnAttribute = "DomainModelDependsOnAttribute";

  public static AnalysisResult<ModelMetadata> Collect(GeneratorAttributeSyntaxContext source, CancellationToken _)
  {
    var result = new AnalysisResult<ModelMetadata>();
    var modelSymbol =
      source.TargetSymbol as INamedTypeSymbol
      ?? throw new CollectModelsException("symbol");

    var modelAttr =
      modelSymbol.GetAttributesOf($"{GeneratorNamespace}.{TargetAttribute}").SingleOrDefault()
      ?? throw new CollectModelsException("modelAttr");
    var domainName = modelAttr.GetArgumentAt(0) as string;
    if (domainName is null or "")
    {
      result.Diagnostics.Add(
        Diagnostic.Create(DiagnosticDescriptors.EmptyStringNotAllowed, modelAttr.GetLocationAt(0), "domainName")
      );
    }

    var dependsOnAttrs =
      modelSymbol.GetAttributesOf($"{GeneratorNamespace}.{DependsOnAttribute}");
    var dependencies = new List<ModelDependency>();
    foreach (var attr in dependsOnAttrs)
    {
      var (dependency, errorDiagnostic) = GetDependency(attr);
      if (dependency is not null) dependencies.Add(dependency);
      if (errorDiagnostic is not null) result.Diagnostics.Add(errorDiagnostic);
    }

    if (result.HasErrorDiagnostic()) return result;
    // If this condition is true, error diagnostics should have been returned,
    // and if this is met, it's something wrong.
    if (domainName is null or "") throw new CollectModelsException("domainName");
    return result with
    {
      Result = new ModelMetadata { DomainName = domainName, PartialModel = modelSymbol, Dependencies = dependencies },
    };
  }

  private static (ModelDependency?, Diagnostic?) GetDependency(AttributeData dependencyAttribute)
  {
    var dependsOn = dependencyAttribute.GetArgumentAt(0) as INamedTypeSymbol
                    ?? throw new CollectModelsException("dependsOn");
    var mappedName = dependencyAttribute.GetArgumentAt(1) as string;
    mappedName ??= dependsOn.Name;
    if (mappedName is "")
    {
      var diagnostic = Diagnostic.Create(
        DiagnosticDescriptors.EmptyStringNotAllowed, dependencyAttribute.GetLocationAt(1), "mappedName"
      );
      return (null, diagnostic);
    }

    return (new ModelDependency { DependsOn = dependsOn, MappedName = mappedName }, null);
  }
}

internal record ModelMetadata
{
  public string DomainName { get; set; } = null!;
  public INamedTypeSymbol? PartialModel { get; set; }
  public IEnumerable<ModelDependency> Dependencies { get; set; } = [];

  public string ModelName => PartialModel?.Name ?? $"{DomainName}Domain";
}

internal record ModelDependency
{
  public INamedTypeSymbol DependsOn { get; set; } = null!;
  public string MappedName { get; set; } = null!;
}

internal class CollectModelsException(string segment) : InvalidOperationException(segment);
