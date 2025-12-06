using EFCore.DomainModelGenerator.AnalysisResult;
using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.Emissions;

internal static class ReportDiagnostics
{
  public static void Report<T>(SourceProductionContext context, AnalysisResult<T> analyzedResult)
  {
    foreach (var diagnostic in analyzedResult.Diagnostics)
    {
      context.ReportDiagnostic(diagnostic);
    }
  }
}
