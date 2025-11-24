using Microsoft.CodeAnalysis;

namespace EFCore.DomainModelGenerator;

[Generator(LanguageNames.CSharp)]
public class SampleGenerator : IIncrementalGenerator
{
  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    context.RegisterPostInitializationOutput(static ctx =>
      ctx.AddSource("DomainContextAttribute.g.cs", DomainContextAttributeSource.Source));
    var cnt = 0;
    var source = context.SyntaxProvider.CreateSyntaxProvider(
      (_, _) => cnt++ == 0,
      static (ctx, _) => ctx);
    context.RegisterSourceOutput(source, Emit);
  }

  private static void Emit(SourceProductionContext context, GeneratorSyntaxContext source)
  {
    const string code =
      """
      namespace SampleWorker;
      using System;

      public class SampleClass
      {
        public void SayHello()
        {
          Console.WriteLine("Hello from generated class!");
        }
      }
      """;
    context.AddSource("SampleClass.g.cs", code);
  }
}
