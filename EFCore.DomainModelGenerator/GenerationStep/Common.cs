using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.GenerationStep;

internal static class Common
{
  public const string GeneratorNamespace = "global::EFCore.DomainModelGenerator";

  public static T? GetArgumentAt<T>(this AttributeData attribute, int index) where T : class
  {
    return attribute.ConstructorArguments.ElementAtOrDefault(index) as T;
  }

  public static IEnumerable<AttributeData> GetAttributesOf(this ISymbol symbol, string attributeName)
  {
    return symbol.GetAttributes().Where(x =>
      x.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == attributeName);
  }
}
