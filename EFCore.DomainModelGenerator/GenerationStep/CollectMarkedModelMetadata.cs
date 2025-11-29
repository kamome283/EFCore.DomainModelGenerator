using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.GenerationStep;

using static Common;

internal static class CollectMarkedModelMetadata
{
  public const string TargetAttribute = "DomainModelDependsOn";

  public static MarkedModelMetadata Collect(GeneratorAttributeSyntaxContext source, CancellationToken _)
  {
    var symbol = source.TargetSymbol as INamedTypeSymbol ?? throw new CollectMarkedModelMetadataException("symbol");
    var modelName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    var dependencies = symbol
      .GetAttributesOf($"{GeneratorNamespace}.{TargetAttribute}")
      .Select(GetDependency);
    return new MarkedModelMetadata { ModelName = modelName, Dependencies = dependencies };
  }

  private static (string DependsOn, string MappedName) GetDependency(AttributeData dependencyAttribute)
  {
    var dependsOn = dependencyAttribute.GetArgumentAt<Type>(0)
                    ?? throw new CollectMarkedModelMetadataException("dependsOn");
    var mappedName = dependencyAttribute.GetArgumentAt<string>(1);
    mappedName ??= dependsOn.Name;
    return (dependsOn.FullName, mappedName);
  }
}

internal record MarkedModelMetadata
{
  public string ModelName { get; set; } = null!;
  public IEnumerable<(string DependsOn, string MappedName)> Dependencies { get; set; } = [];
}

file class CollectMarkedModelMetadataException(string segment) : Exception(segment);
