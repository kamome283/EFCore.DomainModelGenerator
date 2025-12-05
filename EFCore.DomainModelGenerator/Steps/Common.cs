using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.Steps;

internal static class Common
{
  public const string GeneratorNamespace = "global::EFCore.DomainModelGenerator";

  public static IEnumerable<AttributeData> GetAttributesOf(this ISymbol symbol, string fullyQualifiedName)
  {
    return symbol.GetAttributes().Where(x =>
      x.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == fullyQualifiedName);
  }

  public static object? GetArgumentAt(this AttributeData attribute, int index)
  {
    return attribute.ConstructorArguments.ElementAtOrDefault(index).Value;
  }
}
