using EFCore.DomainModelGenerator.AnalysisResult;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EFCore.DomainModelGenerator.Steps;

internal static class ReadConfig
{
  private const string DomainNamespaceConfigKey = "build_property.EFCoreDomainModelGenerator_DomainNamespace";
  private const string DefaultNamespace = "EFCore.DomainModelGenerator.Domains";

  public static AnalysisResult<GeneratorConfig> Read(AnalyzerConfigOptionsProvider options, CancellationToken _)
  {
    var domainNamespace = options.GlobalOptions.TryGetValue(DomainNamespaceConfigKey, out var maybeDomainNamespace)
      ? maybeDomainNamespace
      : DefaultNamespace;
    return new AnalysisResult<GeneratorConfig>
    {
      Result = new GeneratorConfig { DomainNamespace = domainNamespace },
    };
  }
}

internal record GeneratorConfig
{
  public string DomainNamespace { get; set; } = null!;
}
