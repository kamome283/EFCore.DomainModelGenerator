using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator;

[Generator(LanguageNames.CSharp)]
public partial class SampleGenerator : IIncrementalGenerator
{
  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
  }
}
