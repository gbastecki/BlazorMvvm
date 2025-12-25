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
        private bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is FieldDeclarationSyntax { AttributeLists.Count: > 0 }
                                                                      or MethodDeclarationSyntax { AttributeLists.Count: > 0 }
                                                                      or ClassDeclarationSyntax { AttributeLists.Count: > 0 };

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
            else if (syntaxNode is ClassDeclarationSyntax classDeclaration)
            {
                INamedTypeSymbol? symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
                if (symbol != null && HasAttribute(symbol, "BlazorMvvm.BlazorMessengerAttribute"))
                {
                    return symbol;
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
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public partial class {className}");
            sb.AppendLine("    {");

            bool hasMembers = false;
            List<(string CommandName, string CallbackMethod, string CommandInterfaceType)> commandsWithCallback = new();

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
                    (string? commandName, string? callback, string? interfaceType) = GenerateCommand(sb, member, classSymbol);
                    if (commandName != null && callback != null && interfaceType != null)
                    {
                        commandsWithCallback.Add((commandName, callback, interfaceType));
                    }
                }
            }

            // Generate messenger registration if class has BlazorMessengerAttribute
            if (HasAttribute(classSymbol, "BlazorMvvm.BlazorMessengerAttribute"))
            {
                hasMembers = true;
                GenerateMessengerMethods(sb, classSymbol);
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return hasMembers ? sb.ToString() : null;
        }

        private void GenerateProperty(StringBuilder sb, IFieldSymbol fieldSymbol)
        {
            string fieldName = fieldSymbol.Name;
            string propertyType = fieldSymbol.Type.ToDisplayString();

            // Check for custom Name in attribute
            AttributeData attribute = fieldSymbol.GetAttributes().First(ad => ad.AttributeClass!.ToDisplayString() == "BlazorMvvm.BlazorObservablePropertyAttribute");

            string? customName = null;
            foreach (KeyValuePair<string, TypedConstant> namedArg in attribute.NamedArguments)
            {
                if (namedArg.Key == "Name" && namedArg.Value.Value is string name)
                {
                    customName = name;
                    break;
                }
            }

            string? propertyName = !string.IsNullOrEmpty(customName) ? customName : GetPropertyName(fieldName);

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

        private (string? CommandName, string? CallbackMethod, string? CommandInterfaceType) GenerateCommand(StringBuilder sb, IMethodSymbol methodSymbol, INamedTypeSymbol classSymbol)
        {
            AttributeData attribute = methodSymbol.GetAttributes().First(ad => ad.AttributeClass!.ToDisplayString() == "BlazorMvvm.BlazorCommandAttribute");

            string methodName = methodSymbol.Name;
            string commandName = $"{methodName}Command";
            string commandFieldName = $"_{char.ToLower(commandName[0])}{commandName.Substring(1)}";

            string? canExecute = null;
            object? allowConcurrentExecutionsObj = null;
            string? onIsExecutingChangedCallback = null;
            bool autoRefreshOnIsExecutingChanged = false;

            if (attribute.ConstructorArguments.Length > 0 && !attribute.ConstructorArguments[0].IsNull)
            {
                canExecute = attribute.ConstructorArguments[0].Value as string;
            }
            if (attribute.ConstructorArguments.Length > 1 && !attribute.ConstructorArguments[1].IsNull)
            {
                allowConcurrentExecutionsObj = attribute.ConstructorArguments[1].Value;
            }
            if (attribute.ConstructorArguments.Length > 2 && !attribute.ConstructorArguments[2].IsNull)
            {
                onIsExecutingChangedCallback = attribute.ConstructorArguments[2].Value as string;
            }
            if (attribute.ConstructorArguments.Length > 3 && !attribute.ConstructorArguments[3].IsNull)
            {
                autoRefreshOnIsExecutingChanged = attribute.ConstructorArguments[3].Value is bool b && b;
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
                else if (namedArg.Key == "OnIsExecutingChangedCallback")
                {
                    onIsExecutingChangedCallback = namedArg.Value.Value as string;
                }
                else if (namedArg.Key == "AutoRefreshOnIsExecutingChanged")
                {
                    autoRefreshOnIsExecutingChanged = namedArg.Value.Value is bool b && b;
                }
            }

            string? allowConcurrentExecutions = null;
            if (allowConcurrentExecutionsObj is bool b2) allowConcurrentExecutions = b2.ToString().ToLower();
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

            // Determine if factory method is needed (for callbacks)
            bool needsCallback = isAsync && (!string.IsNullOrEmpty(onIsExecutingChangedCallback) || autoRefreshOnIsExecutingChanged);

            if (needsCallback)
            {
                string factoryMethodName = $"Create{commandName}";

                sb.AppendLine();
                sb.AppendLine($"        private {commandInterfaceType}? {commandFieldName};");
                sb.AppendLine($"        public {commandInterfaceType} {commandName} => {commandFieldName} ??= {factoryMethodName}();");
                sb.AppendLine();
                sb.AppendLine($"        private {commandInterfaceType} {factoryMethodName}()");
                sb.AppendLine("        {");
                sb.Append($"            var cmd = new {commandImplementationType}");
                if (!string.IsNullOrEmpty(commandGenericType))
                {
                    sb.Append($"<{commandGenericType}>");
                }
                sb.Append("(");

                GenerateCommandExecuteArgument(sb, methodSymbol, methodName, parameters, isAsync);

                // generate CanExecute logic
                if (canExecute != null)
                {
                    string canExecuteParam = GenerateCanExecuteArgument(canExecute, classSymbol, isAsync);
                    sb.Append($", canExecute: {canExecuteParam}");
                }

                if (allowConcurrentExecutions != null)
                {
                    sb.Append($", allowConcurrentExecutions: {allowConcurrentExecutions}");
                }

                sb.AppendLine(");");

                // Generate callback handler based on what is enabled
                if (autoRefreshOnIsExecutingChanged && !string.IsNullOrEmpty(onIsExecutingChangedCallback))
                {
                    // Both auto-refresh and custom callback
                    sb.AppendLine($"            cmd.OnIsExecutingChanged += isExec => {{ OnPropertyChanged(); {onIsExecutingChangedCallback}(isExec); }};");
                }
                else if (autoRefreshOnIsExecutingChanged)
                {
                    // Only auto-refresh
                    sb.AppendLine($"            cmd.OnIsExecutingChanged += _ => OnPropertyChanged();");
                }
                else
                {
                    // Only custom callback
                    sb.AppendLine($"            cmd.OnIsExecutingChanged += {onIsExecutingChangedCallback};");
                }

                sb.AppendLine("            return cmd;");
                sb.AppendLine("        }");

                return (commandName, onIsExecutingChangedCallback, commandInterfaceType);
            }
            else
            {
                // generation without callback
                sb.AppendLine();
                sb.AppendLine($"        private {commandInterfaceType}? {commandFieldName};");
                sb.Append($"        public {commandInterfaceType} {commandName} => {commandFieldName} ??= new {commandImplementationType}");
                if (!string.IsNullOrEmpty(commandGenericType))
                {
                    sb.Append($"<{commandGenericType}>");
                }

                sb.Append("(");

                GenerateCommandExecuteArgument(sb, methodSymbol, methodName, parameters, isAsync);

                // generate CanExecute logic
                if (canExecute != null)
                {
                    string canExecuteParam = GenerateCanExecuteArgument(canExecute, classSymbol, isAsync);
                    sb.Append($", canExecute: {canExecuteParam}");
                }

                if (isAsync && allowConcurrentExecutions != null)
                {
                    sb.Append($", allowConcurrentExecutions: {allowConcurrentExecutions}");
                }

                sb.AppendLine(");");
            }

            return (null, null, null);
        }

        private void GenerateCommandExecuteArgument(StringBuilder sb, IMethodSymbol methodSymbol, string methodName, ImmutableArray<IParameterSymbol> parameters, bool isAsync)
        {
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
        }

        private string GenerateCanExecuteArgument(string canExecute, INamedTypeSymbol classSymbol, bool isAsync)
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

            return canExecuteParam;
        }

        private void GenerateMessengerMethods(StringBuilder sb, INamedTypeSymbol classSymbol)
        {
            // Find all IBlazorRecipient<TMessage> implementations
            List<INamedTypeSymbol> recipientInterfaces = new();

            foreach (INamedTypeSymbol iface in classSymbol.AllInterfaces)
            {
                if (iface.IsGenericType &&
                    iface.ConstructedFrom.ToDisplayString().StartsWith("BlazorMvvm.IBlazorRecipient<"))
                {
                    recipientInterfaces.Add(iface);
                }
            }

            if (recipientInterfaces.Count == 0) return;

            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Registers this instance with the specified messenger for all implemented message types.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public void RegisterMessenger(BlazorMvvm.IBlazorMessenger messenger)");
            sb.AppendLine("        {");

            foreach (INamedTypeSymbol iface in recipientInterfaces)
            {
                string messageType = iface.TypeArguments[0].ToDisplayString();
                sb.AppendLine($"            messenger.Register<{messageType}>(this, static (r, m) => (({iface.ToDisplayString()})r).Receive(m));");
            }

            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Unregisters this instance from all message types on the specified messenger.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public void UnregisterMessenger(BlazorMvvm.IBlazorMessenger messenger)");
            sb.AppendLine("        {");
            sb.AppendLine("            messenger.UnregisterAll(this);");
            sb.AppendLine("        }");
        }
    }
}
