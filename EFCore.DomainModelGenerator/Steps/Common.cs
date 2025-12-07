using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EFCore.DomainModelGenerator.Steps;

internal static class Common
{
  public const string GeneratorNamespace = "global::EFCore.DomainModelGenerator";

  public static IEnumerable<AttributeData> GetAttributesOf(this ISymbol symbol, string fullyQualifiedName)
  {
    return symbol.GetAttributes().Where(x =>
      x.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == fullyQualifiedName);
  }

  extension(AttributeData attribute)
  {
    public object? GetArgumentAt(int index)
    {
      return attribute.ConstructorArguments.ElementAtOrDefault(index).Value;
    }

    public Location? GetLocationAt(int index)
    {
      var syntax = attribute.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax;
      return syntax?.ArgumentList?.Arguments.ElementAtOrDefault(index)?.GetLocation()
             ?? syntax?.GetLocation();
    }
  }
}
