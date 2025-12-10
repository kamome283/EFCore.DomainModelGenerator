using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator.AnalysisResult;

internal record AnalysisResult<T>
{
  public T? Result { get; set; }
  public List<Diagnostic> Diagnostics { get; } = [];

  public bool HasErrorDiagnostic() =>
    Diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);
}
