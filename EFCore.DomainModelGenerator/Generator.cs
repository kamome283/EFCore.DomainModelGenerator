using EFCore.DomainModelGenerator.ConstantSource;
using EFCore.DomainModelGenerator.Emissions;
using EFCore.DomainModelGenerator.Steps;
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
    var setSource = context.SyntaxProvider.ForAttributeWithMetadataName(
      $"{GeneratorNamespace}.{CollectSetMetadata.TargetAttribute}",
      static (_, _) => true,
      static (context, _) => context);
    var contextSource = context.SyntaxProvider.ForAttributeWithMetadataName(
      $"{GeneratorNamespace}.{CollectContextMetadata.TargetAttribute}",
      static (_, _) => true,
      static (context, _) => context);

    var markedModelMetadatum = markedModelSource.Select(CollectMarkedModelMetadata.Collect).Collect();
    var setMetadatum = setSource.Select(CollectSetMetadata.Collect).Collect();
    var contextMetadatum = contextSource.Select(CollectContextMetadata.Collect).Collect();

    var groups = contextMetadatum
      .Combine(setMetadatum)
      .Combine(markedModelMetadatum)
      .Select(static (tuple, token) =>
      {
        var (pair, models) = tuple;
        var (contexts, sets) = pair;
        return CombineMetadata.Combine(contexts, models, sets, token);
      });

    context.RegisterSourceOutput(groups, ModelEmission.Emit);
    context.RegisterSourceOutput(groups, RegistratorEmission.Emit);
  }
}
