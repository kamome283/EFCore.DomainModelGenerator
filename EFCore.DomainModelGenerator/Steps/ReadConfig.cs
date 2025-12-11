using EFCore.DomainModelGenerator.AnalysisResult;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EFCore.DomainModelGenerator.Steps;

internal static class ReadConfig
{
  private const string ModelNamespaceConfigKey = "build_property.EFCoreDomainModelGenerator_ModelNamespace";
  private const string DefaultNamespace = "EFCore.DomainModelGenerator.Domains";

  public static AnalysisResult<GeneratorConfig> Read(AnalyzerConfigOptionsProvider options, CancellationToken _)
  {
    var modelNamespace = options.GlobalOptions.TryGetValue(ModelNamespaceConfigKey, out var maybeModelNamespace)
      ? maybeModelNamespace
      : DefaultNamespace;
    modelNamespace = modelNamespace.Trim();
    return new AnalysisResult<GeneratorConfig>
    {
      Result = new GeneratorConfig { ModelNamespace = modelNamespace },
    };
  }
}

internal record GeneratorConfig
{
  public string ModelNamespace { get; set; } = null!;
}
