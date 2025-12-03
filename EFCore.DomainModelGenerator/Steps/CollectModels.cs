using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.Steps;

using static Common;

internal static class CollectModels
{
  public const string TargetAttribute = "DomainModelDependsOn";

  public static ModelMetadata Collect(GeneratorAttributeSyntaxContext source, CancellationToken _)
  {
    var symbol = source.TargetSymbol as INamedTypeSymbol ?? throw new CollectModelsException("symbol");
    var modelName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    var dependencies = symbol
      .GetAttributesOf($"{GeneratorNamespace}.{TargetAttribute}")
      .Select(GetDependency);
    return new ModelMetadata { ModelName = modelName, Dependencies = dependencies };
  }

  private static (string DependsOn, string MappedName) GetDependency(AttributeData dependencyAttribute)
  {
    var dependsOn = dependencyAttribute.GetArgumentAt(0) as Type
                    ?? throw new CollectModelsException("dependsOn");
    var mappedName = dependencyAttribute.GetArgumentAt(1) as string;
    mappedName ??= dependsOn.Name;
    return (dependsOn.FullName, mappedName);
  }
}

internal record ModelMetadata
{
  public string ModelName { get; set; } = null!;
  public IEnumerable<(string DependsOn, string MappedName)> Dependencies { get; set; } = [];
}

internal class CollectModelsException(string segment) : InvalidOperationException(segment);
