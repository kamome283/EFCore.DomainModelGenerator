using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.AnalysisResult;

internal static class EmissionHelper
{
  public static Action<SourceProductionContext, AnalysisResult<T>> AdaptForAnalysisResult<T>(
    this Action<SourceProductionContext, T> action)
  {
    return (context, result) =>
    {
      if (result.Result is null) return;
      action(context, result.Result);
    };
  }
}
