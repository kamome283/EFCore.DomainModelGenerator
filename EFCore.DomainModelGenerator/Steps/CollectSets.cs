using EFCore.DomainModelGenerator.AnalysisResult;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EFCore.DomainModelGenerator.Steps;

using static Common;

internal static class CollectSets
{
  public const string TargetAttribute = "DomainSetAttribute";

  public static AnalysisResult<SetMetadata> Collect(GeneratorAttributeSyntaxContext source, CancellationToken _)
  {
    var symbol = source.TargetSymbol as IPropertySymbol ?? throw new CollectSetsException("symbol");
    var propType = symbol.Type as INamedTypeSymbol;
    if (propType is null or not { IsGenericType: true, ConstructedFrom.Name: "DbSet" })
      throw new CollectSetsException("propType");
    var attr = symbol.GetAttributesOf($"{GeneratorNamespace}.{TargetAttribute}").SingleOrDefault()
               ?? throw new CollectSetsException("attr");

    var parentType =
      symbol.ContainingType as ITypeSymbol
      ?? throw new CollectSetsException("ParentType");
    var domainContextAttr =
      parentType.GetAttributesOf($"{GeneratorNamespace}.{CollectContexts.TargetAttribute}").SingleOrDefault();
    if (domainContextAttr is null)
    {
      var node =
        source.TargetNode as PropertyDeclarationSyntax
        ?? throw new CollectSetsException("node");
      var diagnostic =
        Diagnostic.Create(
          DiagnosticDescriptors.DomainSetOutsideDomainContext, node.Identifier.GetLocation(), symbol.Name);
      return new AnalysisResult<SetMetadata> { Diagnostics = [diagnostic] };
    }

    return new AnalysisResult<SetMetadata>
    {
      Result = new SetMetadata
      {
        ParentType = symbol.ContainingType as ITypeSymbol ??
                     throw new CollectSetsException("ParentType"),
        DomainName = attr.GetArgumentAt(0) as string ?? symbol.Name,
        MappedName = attr.GetArgumentAt(1) as string ?? symbol.Name,
        OriginalName = symbol.Name,
        ElementType = propType.TypeArguments.Single(),
        ReadonlyAccessibility = GetAccessibility(attr.GetArgumentAt(2) as int?)
                                ?? throw new CollectSetsException("ReadonlyAccessibility"),
        WritableAccessibility = GetAccessibility(attr.GetArgumentAt(3) as int?)
                                ?? throw new CollectSetsException("WritableAccessibility"),
      },
    };
  }

  private static string GetAccessibility(int? accessibility)
  {
    return accessibility switch
    {
      0 => "public",
      1 => "internal",
      2 => "protected",
      _ => throw new CollectSetsException("accessibility"),
    };
  }
}

internal record SetMetadata
{
  public ITypeSymbol ParentType { get; set; } = null!;
  public string DomainName { get; set; } = null!;
  public string MappedName { get; set; } = null!;
  public string OriginalName { get; set; } = null!;
  public ITypeSymbol ElementType { get; set; } = null!;
  public string ReadonlyAccessibility { get; set; } = null!;
  public string WritableAccessibility { get; set; } = null!;
}

internal class CollectSetsException(string segment) : InvalidOperationException(segment);
