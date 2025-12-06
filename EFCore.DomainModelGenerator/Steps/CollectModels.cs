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
    var modelSymbol =
      source.TargetSymbol as INamedTypeSymbol
      ?? throw new CollectModelsException("symbol");

    var modelAttr =
      modelSymbol.GetAttributesOf($"{GeneratorNamespace}.{TargetAttribute}").SingleOrDefault()
      ?? throw new CollectModelsException("modelAttr");
    var domainName =
      modelAttr.GetArgumentAt(0) as string
      ?? throw new CollectModelsException("domainName");

    var dependsOnAttrs =
      modelSymbol.GetAttributesOf($"{GeneratorNamespace}.{DependsOnAttribute}");
    var dependencies =
      dependsOnAttrs.Select(GetDependency);

    return new AnalysisResult<ModelMetadata>
    {
      Result = new ModelMetadata { DomainName = domainName, PartialModel = modelSymbol, Dependencies = dependencies },
    };
  }

  private static ModelDependency GetDependency(AttributeData dependencyAttribute)
  {
    var dependsOn = dependencyAttribute.GetArgumentAt(0) as Type
                    ?? throw new CollectModelsException("dependsOn");
    var mappedName = dependencyAttribute.GetArgumentAt(1) as string;
    mappedName ??= dependsOn.Name;
    return new ModelDependency { DependsOn = dependsOn, MappedName = mappedName };
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
  public Type DependsOn { get; set; } = null!;
  public string MappedName { get; set; } = null!;
}

internal class CollectModelsException(string segment) : InvalidOperationException(segment);
