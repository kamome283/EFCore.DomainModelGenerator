using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.AnalysisResult;

internal record AnalysisResult<T>
{
  public T? Result { get; set; }
  public IEnumerable<Diagnostic> Diagnostics { get; set; } = new List<Diagnostic>();
}
