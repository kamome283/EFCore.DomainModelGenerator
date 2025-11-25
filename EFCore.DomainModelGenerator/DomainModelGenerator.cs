using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator;

[Generator(LanguageNames.CSharp)]
public class DomainModelGenerator : IIncrementalGenerator
{
  private const string GeneratorNamespace = "EFCore.DomainModelGenerator";

  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    context.RegisterPostInitializationOutput(static ctx =>
    {
      ctx.AddSource("DomainContextAttribute.g.cs", DomainContextAttributeSource.Source);
      ctx.AddSource("DomainSetAttribute.g.cs", DomainSetAttributeSource.Source);
    });
    var source = context.SyntaxProvider.ForAttributeWithMetadataName(
      $"{GeneratorNamespace}.DomainContextAttribute",
      static (_, _) => true,
      static (context, _) => context);
    context.RegisterSourceOutput(source, Emit);
  }

  private static void Emit(SourceProductionContext context, GeneratorAttributeSyntaxContext source)
  {
    var typeSymbol = (INamedTypeSymbol)source.TargetSymbol;

    var domainContextAttribute = typeSymbol.GetAttributes().SingleOrDefault(attr =>
    {
      var name = attr
        .AttributeClass
        ?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      return name?.Contains($"{GeneratorNamespace}.DomainContextAttribute") ?? false;
    });
    if (domainContextAttribute is null)
      throw new InvalidOperationException("DomainContextAttribute is not given to this class");

    var ns = domainContextAttribute
      .ConstructorArguments
      .FirstOrDefault()
      .Value as string;
    if (ns is null) throw new InvalidOperationException("Domain namespace is not specified in DomainContextAttribute");

    var domainSets = typeSymbol
      .GetMembers()
      .SelectMany(sym =>
      {
        var metadata = GetDomainSetMetadata(sym);
        return metadata is not null ? new[] { metadata } : [];
      });

    foreach (var group in domainSets.GroupBy(x => x.DomainName))
    {
      var domainClassName = $"{group.Key}Domain";
      var domainModelSource = new DomainModelSource
      {
        Namespace = ns,
        ContextType = typeSymbol,
        DomainSetMetadata = group,
        DomainClassName = domainClassName,
      };
      var code = domainModelSource.GenerateCode();
      context.AddSource($"{domainClassName}.g.cs", code);
    }
  }

  private static DomainSetMetadata? GetDomainSetMetadata(ISymbol symbol)
  {
    if (symbol is not IPropertySymbol
        {
          IsStatic: false,
          DeclaredAccessibility: Accessibility.Public or Accessibility.Internal,
          CanBeReferencedByName: true,
        } prop) return null;

    var attr = prop.GetAttributes().FirstOrDefault(x =>
    {
      var name = x
        .AttributeClass
        ?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      return name?.Contains($"{GeneratorNamespace}.DomainSetAttribute") ?? false;
    });
    if (attr is null) return null;

    var domainName = attr
      .ConstructorArguments
      .ElementAtOrDefault(0)
      .Value as string;
    domainName ??= prop.Name;

    var isPrivate = attr
      .ConstructorArguments
      .ElementAtOrDefault(1)
      .Value is true;

    return new DomainSetMetadata
    {
      DomainName = domainName,
      OriginalName = prop.Name,
      MappedName = prop.Name,
      ElementType = prop.Type,
      IsPrivate = isPrivate,
    };
  }
}
