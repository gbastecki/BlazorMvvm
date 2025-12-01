using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace BlazorMvvm.Generator.Generators
{
    [Generator]
    public class BlazorViewModelFactoryGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<ISymbol?> classDeclarationsProvider = context.SyntaxProvider.CreateSyntaxProvider(predicate: (syntaxNode, _) => IsSyntaxTargetForGeneration(syntaxNode),
                                                                                                                        transform: (generatorSyntaxContext, _) => GetSemanticTargetForGeneration(generatorSyntaxContext))
                                                                                                  .Where(static symbol => symbol is not null);

            IncrementalValueProvider<ImmutableArray<ISymbol?>> collectedClasses = classDeclarationsProvider.Collect();
            var source = collectedClasses.Combine(context.CompilationProvider);
            context.RegisterSourceOutput(source, (sourceProductionContext, source) => Execute(sourceProductionContext, source.Left, source.Right));
        }

        private bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

        private ISymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
            ISymbol? symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

            if (symbol != null && HasAttribute(symbol, "BlazorMvvm.BlazorMvvmViewModelAttribute"))
            {
                return symbol;
            }

            return null;
        }

        private bool HasAttribute(ISymbol symbol, string attributeName)
        {
            return symbol.GetAttributes().Any(attributeData => attributeData.AttributeClass != null &&
                                                               attributeData.AttributeClass.ToDisplayString() == attributeName);
        }

        private void Execute(SourceProductionContext context, ImmutableArray<ISymbol?> symbols, Compilation compilation)
        {
            StringBuilder sb = new();
            sb.AppendLine("using BlazorMvvm;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine();
            sb.AppendLine($"namespace BlazorMvvm");
            sb.AppendLine("{");

            // Generate ModuleInitializer for current assembly
            var assemblyName = compilation.AssemblyName?.Replace(".", "_");
            sb.AppendLine($"    public static class BlazorMvvmRegistrator_{assemblyName}");
            sb.AppendLine("    {");
            sb.AppendLine("        [ModuleInitializer]");
            sb.AppendLine("        public static void Initialize()");
            sb.AppendLine("        {");

            if (symbols.Any())
            {
                foreach (var symbol in symbols.Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>())
                {
                    // Find constructor:
                    // marked with [BlazorMvvmViewModelFactoryConstructor]
                    // OR constructor with most parameters
                    var ctors = symbol.Constructors;
                    var ctor = ctors.FirstOrDefault(c => HasAttribute(c, "BlazorMvvm.BlazorMvvmViewModelFactoryConstructorAttribute"));

                    ctor ??= ctors.OrderByDescending(c => c.Parameters.Length).FirstOrDefault();

                    if (ctor != null)
                    {
                        var args = string.Join(", ", ctor.Parameters.Select(p =>
                        {
                            var typeName = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                            if (p.NullableAnnotation == NullableAnnotation.Annotated)
                            {
                                string typeForService;
                                if (p.Type.IsReferenceType)
                                {
                                    typeForService = p.Type.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                                }
                                else
                                {
                                    typeForService = typeName;
                                }
                                return $"({typeName})sp.GetService(typeof({typeForService}))";
                            }
                            else
                            {
                                return $"sp.GetRequiredService<{typeName}>()";
                            }
                        }));

                        // Get ViewModel lifetime
                        var attribute = symbol.GetAttributes().First(ad => ad.AttributeClass!.ToDisplayString() == "BlazorMvvm.BlazorMvvmViewModelAttribute");
                        int lifetime = 0; // default Transient
                        if (attribute.ConstructorArguments.Length > 0)
                        {
                            lifetime = (int)attribute.ConstructorArguments[0].Value!;
                        }
                        string lifetimeEnum = lifetime switch
                        {
                            1 => "ViewModelLifetime.Scoped",
                            2 => "ViewModelLifetime.Singleton",
                            _ => "ViewModelLifetime.Transient"
                        };

                        sb.AppendLine($"            ViewModelRegistry.Register(typeof({symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}), sp => new {symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}({args}), {lifetimeEnum});");
                    }
                }
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");

            // Generate main entry point (factory and cache)
            sb.AppendLine();
            sb.AppendLine("    internal static class BlazorMvvmViewModelFactoryExtensions");
            sb.AppendLine("    {");
            sb.AppendLine("        public static IServiceCollection UseBlazorMvvmViewModelFactory(this IServiceCollection services)");
            sb.AppendLine("        {");
            sb.AppendLine("            services.AddSingleton<IBlazorMvvmViewModelFactory, BlazorMvvmViewModelFactory>();");
            sb.AppendLine("            services.AddScoped<BlazorMvvmScopedCache>();");
            sb.AppendLine("            return services;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            sb.AppendLine("}");

            context.AddSource($"BlazorMvvm.BlazorMvvmRegistrator.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }
}
