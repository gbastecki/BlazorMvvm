using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    public class BlazorViewModelGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<ISymbol?> classDeclarationsProvider = context.SyntaxProvider.CreateSyntaxProvider(predicate: (syntaxNode, _) => IsSyntaxTargetForGeneration(syntaxNode), 
                                                                                                                        transform: (generatorSyntaxContext, _) => GetSemanticTargetForGeneration(generatorSyntaxContext))
                                                                                                  .Where(static namedTypeSymbol => namedTypeSymbol is not null)
                                                                                                  .Collect()
                                                                                                  .SelectMany((classes, _) => classes.Distinct(SymbolEqualityComparer.Default));
            context.RegisterSourceOutput(classDeclarationsProvider, (sourceProductionContext, symbol) => Execute(sourceProductionContext, symbol as INamedTypeSymbol));
        }
        private bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is FieldDeclarationSyntax { AttributeLists.Count: > 0 } or MethodDeclarationSyntax { AttributeLists.Count: > 0 };

        private INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            SyntaxNode syntaxNode = context.Node;

            if (syntaxNode is FieldDeclarationSyntax fieldDeclaration)
            {
                foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
                {
                    ISymbol? symbol = context.SemanticModel.GetDeclaredSymbol(variable);
                    if (symbol != null && HasAttribute(symbol, "BlazorMvvm.BlazorObservablePropertyAttribute"))
                    {
                        return symbol.ContainingType;
                    }
                }
            }
            else if (syntaxNode is MethodDeclarationSyntax methodDeclaration)
            {
                IMethodSymbol? symbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
                if (symbol != null && HasAttribute(symbol, "BlazorMvvm.BlazorCommandAttribute"))
                {
                    return symbol.ContainingType;
                }
            }

            return null;
        }

        private bool HasAttribute(ISymbol symbol, string attributeName)
        {
            return symbol.GetAttributes().Any(attributeData => attributeData.AttributeClass != null &&
                                                               attributeData.AttributeClass.ToDisplayString() == attributeName);
        }

        private void Execute(SourceProductionContext context, INamedTypeSymbol? classSymbol)
        {
            if (classSymbol == null) return;
            string? classSource = GenerateClassSource(classSymbol);
            if (classSource != null)
            {
                context.AddSource($"{classSymbol.Name}.g.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private string? GenerateClassSource(INamedTypeSymbol classSymbol)
        {
            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            string className = classSymbol.Name;

            StringBuilder sb = new();
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public partial class {className}");
            sb.AppendLine("    {");

            bool hasMembers = false;

            // fields
            foreach (IFieldSymbol member in classSymbol.GetMembers().OfType<IFieldSymbol>())
            {
                if (HasAttribute(member, "BlazorMvvm.BlazorObservablePropertyAttribute"))
                {
                    hasMembers = true;
                    GenerateProperty(sb, member);
                }
            }

            // methods
            foreach (IMethodSymbol member in classSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                if (HasAttribute(member, "BlazorMvvm.BlazorCommandAttribute"))
                {
                    hasMembers = true;
                    GenerateCommand(sb, member, classSymbol);
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return hasMembers ? sb.ToString() : null;
        }

        private void GenerateProperty(StringBuilder sb, IFieldSymbol fieldSymbol)
        {
            string fieldName = fieldSymbol.Name;
            string propertyName = GetPropertyName(fieldName);
            string propertyType = fieldSymbol.Type.ToDisplayString();

            sb.AppendLine();
            sb.AppendLine($"        public {propertyType} {propertyName}");
            sb.AppendLine("        {");
            sb.AppendLine($"            get => {fieldName};");
            sb.AppendLine($"            set => Set(ref {fieldName}, value);");
            sb.AppendLine("        }");
        }

        private string GetPropertyName(string fieldName)
        {
            if (fieldName.StartsWith("m_"))
            {
                fieldName = fieldName.Substring(2);
            }
            else if (fieldName.StartsWith("_"))
            {
                fieldName = fieldName.TrimStart('_');
            }
            if (fieldName.Length == 0) return "Property";
            if (fieldName.Length == 1) return fieldName.ToUpper();
            return char.ToUpper(fieldName[0]) + fieldName.Substring(1);
        }

        private void GenerateCommand(StringBuilder sb, IMethodSymbol methodSymbol, INamedTypeSymbol classSymbol)
        {
            AttributeData attribute = methodSymbol.GetAttributes().First(ad => ad.AttributeClass!.ToDisplayString() == "BlazorMvvm.BlazorCommandAttribute");

            string methodName = methodSymbol.Name;
            string commandName = $"{methodName}Command";
            string commandFieldName = $"_{char.ToLower(commandName[0])}{commandName.Substring(1)}";

            string? canExecute = null;
            object? allowConcurrentExecutionsObj = null;

            if (attribute.ConstructorArguments.Length > 0 && !attribute.ConstructorArguments[0].IsNull)
            {
                canExecute = attribute.ConstructorArguments[0].Value as string;
            }
            if (attribute.ConstructorArguments.Length > 1 && !attribute.ConstructorArguments[1].IsNull)
            {
                allowConcurrentExecutionsObj = attribute.ConstructorArguments[1].Value;
            }

            foreach (KeyValuePair<string, TypedConstant> namedArg in attribute.NamedArguments)
            {
                if (namedArg.Key == "CanExecute")
                {
                    canExecute = namedArg.Value.Value as string;
                }
                else if (namedArg.Key == "AllowConcurrentExecutions")
                {
                    allowConcurrentExecutionsObj = namedArg.Value.Value;
                }
            }

            string? allowConcurrentExecutions = null;
            if (allowConcurrentExecutionsObj is bool b) allowConcurrentExecutions = b.ToString().ToLower();
            else if (allowConcurrentExecutionsObj is string s) allowConcurrentExecutions = s;

            bool isAsync = methodSymbol.IsAsync
                           || methodSymbol.ReturnType.ToDisplayString().StartsWith("System.Threading.Tasks.Task")
                           || methodSymbol.ReturnType.ToDisplayString().StartsWith("System.Threading.Tasks.ValueTask");

            ImmutableArray<IParameterSymbol> parameters = methodSymbol.Parameters;

            string commandInterfaceType;
            string commandImplementationType;
            string commandGenericType = "";

            if (parameters.Length == 0)
            {
                if (isAsync)
                {
                    commandInterfaceType = "BlazorMvvm.IBlazorAsyncCommand";
                    commandImplementationType = "BlazorMvvm.BlazorAsyncCommand";
                }
                else
                {
                    commandInterfaceType = "BlazorMvvm.IBlazorCommand";
                    commandImplementationType = "BlazorMvvm.BlazorCommand";
                }
            }
            else if (parameters.Length == 1)
            {
                commandGenericType = parameters[0].Type.ToDisplayString();
                if (isAsync)
                {
                    commandInterfaceType = $"BlazorMvvm.IBlazorAsyncRelayCommand<{commandGenericType}>";
                    commandImplementationType = "BlazorMvvm.BlazorAsyncRelayCommand";
                }
                else
                {
                    commandInterfaceType = $"BlazorMvvm.IBlazorRelayCommand<{commandGenericType}>";
                    commandImplementationType = "BlazorMvvm.BlazorRelayCommand";
                }
            }
            else
            {
                string tupleTypes = string.Join(", ", parameters.Select(p => p.Type.ToDisplayString()));
                commandGenericType = $"({tupleTypes})";

                if (isAsync)
                {
                    commandInterfaceType = $"BlazorMvvm.IBlazorAsyncRelayCommand<{commandGenericType}>";
                    commandImplementationType = "BlazorMvvm.BlazorAsyncRelayCommand";
                }
                else
                {
                    commandInterfaceType = $"BlazorMvvm.IBlazorRelayCommand<{commandGenericType}>";
                    commandImplementationType = "BlazorMvvm.BlazorRelayCommand";
                }
            }

            sb.AppendLine();
            sb.AppendLine($"        private {commandInterfaceType} {commandFieldName};");
            sb.Append($"        public {commandInterfaceType} {commandName} => {commandFieldName} ??= new {commandImplementationType}");
            if (!string.IsNullOrEmpty(commandGenericType))
            {
                sb.Append($"<{commandGenericType}>");
            }

            sb.Append("(");

            if (parameters.Length <= 1)
            {
                if (isAsync && methodSymbol.ReturnType.ToDisplayString().StartsWith("System.Threading.Tasks.ValueTask"))
                {
                    // wrapper for ValueTask
                    if (parameters.Length == 0)
                    {
                        sb.Append($"async () => await {methodName}()");
                    }
                    else
                    {
                        sb.Append($"async arg => await {methodName}(arg)");
                    }
                }
                else
                {
                    sb.Append(methodName);
                }
            }
            else
            {
                // wrapper for tuple
                if (isAsync && methodSymbol.ReturnType.ToDisplayString().StartsWith("System.Threading.Tasks.ValueTask"))
                {
                    sb.Append("async ");
                }

                sb.Append($"args => ");
                if (isAsync && methodSymbol.ReturnType.ToDisplayString().StartsWith("System.Threading.Tasks.ValueTask"))
                {
                    sb.Append("await ");
                }

                sb.Append($"{methodName}(");
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append($"args.Item{i + 1}");
                }
                sb.Append(")");
            }

            // generate CanExecute logic
            if (canExecute != null)
            {
                string canExecuteParam = canExecute;
                IMethodSymbol canExecuteMethod = classSymbol.GetMembers(canExecute).OfType<IMethodSymbol>().FirstOrDefault();

                if (isAsync && canExecuteMethod != null)
                {
                    string returnType = canExecuteMethod.ReturnType.ToDisplayString();
                    if (returnType == "bool" || returnType == "System.Boolean")
                    {
                        // wrap synchronous method for AsyncCommand
                        canExecuteParam = canExecuteMethod.Parameters.Length == 0
                            ? $"() => System.Threading.Tasks.Task.FromResult({canExecute}())"
                            : $"args => System.Threading.Tasks.Task.FromResult({canExecute}(args))";
                    }
                    else if (returnType.StartsWith("System.Threading.Tasks.ValueTask"))
                    {
                        // wrap ValueTask method
                        canExecuteParam = canExecuteMethod.Parameters.Length == 0 ? $"async () => await {canExecute}()" : $"async args => await {canExecute}(args)";
                    }
                }

                sb.Append($", canExecute: {canExecuteParam}");
            }

            if (isAsync && allowConcurrentExecutions != null)
            {
                sb.Append($", allowConcurrentExecutions: {allowConcurrentExecutions}");
            }

            sb.AppendLine(");");
        }
    }
}
