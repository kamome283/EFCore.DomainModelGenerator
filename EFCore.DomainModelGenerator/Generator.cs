using EFCore.DomainModelGenerator.AnalysisResult;
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
      ctx.AddSource("DomainModelAttribute.g.cs", DomainModelAttributeSource.Source);
      ctx.AddSource("Access.g.cs", AccessEnumSource.Source);
      ctx.AddSource("DomainModelDependsOnAttribute.g.cs", DomainModelDependsOnAttributeSource.Source);
    });

    var contextSource = context.SyntaxProvider.ForAttributeWithMetadataName(
      $"{GeneratorNamespace}.{CollectContexts.TargetAttribute}",
      static (_, _) => true,
      static (context, _) => context);

    var setSource = context.SyntaxProvider.ForAttributeWithMetadataName(
      $"{GeneratorNamespace}.{CollectSets.TargetAttribute}",
      static (_, _) => true,
      static (context, _) => context);

    var modelSource = context.SyntaxProvider.ForAttributeWithMetadataName(
      $"{GeneratorNamespace}.{CollectModels.TargetAttribute}",
      static (_, _) => true,
      static (context, _) => context);

    var config = context.AnalyzerConfigOptionsProvider.Select(ReadConfig.Read);
    var contexts = contextSource.Select(CollectContexts.Collect).Collect();
    var sets = setSource.Select(CollectSets.Collect).Collect();
    var models = modelSource.Select(CollectModels.Collect).Collect();

    var groups = config
      .Combine(contexts)
      .Combine(sets)
      .Combine(models)
      .Select(static (ccsm, token) =>
      {
        var (ccs, analyzedModels) = ccsm;
        var (cc, analyzedSets) = ccs;
        var (config, analyzedContexts) = cc;
        return CombineMetadata.Combine(
          config, analyzedContexts.ToArray(), analyzedModels.ToArray(), analyzedSets.ToArray(), token);
      });

    context.RegisterSourceOutput(groups, ReportDiagnostics.Report);
    context.RegisterSourceOutput(
      groups,
      EmissionHelper.AdaptForAnalysisResult<IEnumerable<MetadataGroup>>(ModelEmission.Emit));
    context.RegisterSourceOutput(
      groups,
      EmissionHelper.AdaptForAnalysisResult<IEnumerable<MetadataGroup>>(RegistratorEmission.Emit));
  }
}
