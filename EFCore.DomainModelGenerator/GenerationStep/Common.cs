using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.GenerationStep;

internal static class Common
{
  public const string GeneratorNamespace = "global::EFCore.DomainModelGenerator";

  public static IEnumerable<AttributeData> GetAttributesOf(this ISymbol symbol, string attributeName)
  {
    return symbol.GetAttributes().Where(x =>
      x.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == attributeName);
  }

  extension(AttributeData attribute)
  {
    public object? GetArgumentAt(int index)
    {
      return attribute.ConstructorArguments.ElementAtOrDefault(index).Value;
    }
  }
}
