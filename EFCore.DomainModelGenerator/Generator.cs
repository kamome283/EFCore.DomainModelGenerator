using EFCore.DomainModelGenerator.CodeGeneration;
using EFCore.DomainModelGenerator.ConstantSource;
using EFCore.DomainModelGenerator.GenerationStep;
using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator;

[Generator(LanguageNames.CSharp)]
public class Generator : IIncrementalGenerator
{
  private const string GeneratorNamespace = "EFCore.DomainModelGenerator";

  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    context.RegisterPostInitializationOutput(static ctx =>
    {
      ctx.AddSource("DomainContextAttribute.g.cs", DomainContextAttributeSource.Source);
      ctx.AddSource("DomainSetAttribute.g.cs", DomainSetAttributeSource.Source);
      ctx.AddSource("Access.g.cs", AccessEnumSource.Source);
      ctx.AddSource("DomainModelDependsOnAttribute.g.cs", DomainModelDependsOnAttributeSource.Source);
    });
    var markedModelSource = context.SyntaxProvider.ForAttributeWithMetadataName(
      $"{GeneratorNamespace}.{CollectMarkedModelMetadata.TargetAttribute}",
      static (_, _) => true,
      static (context, _) => context);
    var source = context.SyntaxProvider.ForAttributeWithMetadataName(
      $"{GeneratorNamespace}.DomainContextAttribute",
      static (_, _) => true,
      static (context, _) => context);
    var markedModelMetadatum = markedModelSource.Select(CollectMarkedModelMetadata.Collect);
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

    var domains = domainSets
      .GroupBy(x => x.DomainName)
      .Select(g => new DomainMetadata
      {
        DomainName = $"{g.Key}",
        DomainSetMetadata = g,
      })
      .ToArray();

    // Emission of domain models
    foreach (var domain in domains)
    {
      var domainModelSource = new DomainModelGeneration(ns, typeSymbol, domain);
      context.AddSource($"{domain.DomainName}Domain.g.cs", domainModelSource.GenerateCode());
    }

    // Emission of domain registration helper
    var domainRegistrationSource = new DomainRegistratorGeneration(ns, domains);
    context.AddSource("DomainRegistrationHelper.g.cs", domainRegistrationSource.GenerateCode());
  }

  private static DomainSetMetadata? GetDomainSetMetadata(ISymbol symbol)
  {
    if (symbol is not IPropertySymbol
        {
          IsStatic: false,
          DeclaredAccessibility: Accessibility.Public or Accessibility.Internal,
          CanBeReferencedByName: true,
        } prop) return null;

    if (prop.Type is not INamedTypeSymbol { IsGenericType: true, ConstructedFrom.Name: "DbSet" } propType)
      throw new InvalidOperationException("The type of domain set is not as expected");

    var attr = prop.GetAttributes().FirstOrDefault(x =>
    {
      var name = x
        .AttributeClass
        ?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      return name?.Contains($"{GeneratorNamespace}.DomainSetAttribute") ?? false;
    });
    if (attr is null) return null;

    var elementType =
      propType.TypeArguments.SingleOrDefault() ??
      throw new InvalidOperationException("The count of type arguments of DbSet is not as expected");

    var domainName = attr
      .ConstructorArguments
      .ElementAtOrDefault(0)
      .Value as string;
    domainName ??= prop.Name;

    var mappedName = attr
      .ConstructorArguments
      .ElementAtOrDefault(1)
      .Value as string;
    mappedName ??= prop.Name;

    var readonlyAccessibilityEnumValue = attr
      .ConstructorArguments
      .ElementAtOrDefault(2)
      .Value as int?;
    if (readonlyAccessibilityEnumValue is null)
      throw new InvalidOperationException("failed to get readonlyAccessibility parameter properly");
    var readonlyAccessibility = GetAccessibilityExpr(readonlyAccessibilityEnumValue.Value);

    var writableAccessibilityEnumValue = attr
      .ConstructorArguments
      .ElementAtOrDefault(3)
      .Value as int?;
    if (writableAccessibilityEnumValue is null)
      throw new InvalidOperationException("failed to get writableAccessibility parameter properly");
    var writableAccessibility = GetAccessibilityExpr(writableAccessibilityEnumValue.Value);

    return new DomainSetMetadata
    {
      DomainName = domainName,
      OriginalName = prop.Name,
      MappedName = mappedName,
      ElementType = elementType,
      ReadonlyAccessibility = readonlyAccessibility,
      WritableAccessibility = writableAccessibility,
    };
  }

  private static string GetAccessibilityExpr(int accessibilityEnumValue)
  {
    return accessibilityEnumValue switch
    {
      0 => "public",
      1 => "internal",
      2 => "protected",
      _ => throw new ArgumentOutOfRangeException(nameof(accessibilityEnumValue), accessibilityEnumValue, null),
    };
  }
}
