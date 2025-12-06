using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator;

internal static class DiagnosticDescriptors
{
  public static readonly DiagnosticDescriptor DomainSetOutsideDomainContext = new DiagnosticDescriptor(
    id: "EFDMG0001",
    title: "DomainSet outside DomainContext",
    messageFormat: "Property '{0}' is marked with DomainSet but is defined outside a class marked with DomainContext",
    category: "Usage",
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true);
}
