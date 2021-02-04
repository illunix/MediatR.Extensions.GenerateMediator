using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GenerateMediator
{
    [Generator]
    internal class MediatorGenerator : ISourceGenerator
    {
        private const string attributeText = @"
using System;
namespace GenerateMediator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateMediatorAttribute : Attribute
    {
    }
}
";

        public void Initialize(GeneratorInitializationContext context)
            => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        public void Execute(GeneratorExecutionContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            context.AddSource("GenerateMediatorAttribute", SourceText.From(attributeText, Encoding.UTF8));

            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            var options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(attributeText, Encoding.UTF8), options));

            var attributeSymbol = compilation.GetTypeByMetadataName("GenerateMediator.GenerateMediatorAttribute");
            var classSymbols = new List<INamedTypeSymbol>();

            foreach (var cls in receiver.CandidateClasses)
            {
                var model = compilation.GetSemanticModel(cls.SyntaxTree);

                var classSymbol = model.GetDeclaredSymbol(cls);

                if (classSymbol.GetAttributes().Any(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)))
                {
                    classSymbols.Add(classSymbol);
                }
            }

            foreach (var classSymbol in classSymbols)
            {
                var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

                var sourceBuilder = new StringBuilder();

                var useFluentValidation = false;

                var symbols = new List<ISymbol>
                {
                    classSymbol.GetMembers().FirstOrDefault(x => x.Name == "Query"),
                    classSymbol.GetMembers().FirstOrDefault(x => x.Name == "Command")
                };

                foreach (var symbol in symbols)
                {
                    if (symbol is INamedTypeSymbol prop)
                    {
                        useFluentValidation = prop.GetMembers().Any(x => x.Name == "AddValidation");
                    }
                }

                sourceBuilder.Append(@$"
using MediatR;
{(useFluentValidation == true ? "using FluentValidation;" : "")}
using System.Threading;
using System.Threading.Tasks;

namespace {namespaceName}
{{ 
    public {(classSymbol.IsStatic ? "static" : "")} partial class {classSymbol.Name} 
    {{       
        {GenerateQuerySource(classSymbol)}
        {GenerateCommandSource(classSymbol)}
    }} 
}}");
                context.AddSource($"{classSymbol.Name}.GenerateMediator.g.cs",
                    SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            }
        }

        private static string GenerateQuerySource(INamedTypeSymbol symbol)
        {
            var query = symbol.GetMembers().FirstOrDefault(x => x.Name == "Query");
            if (query is INamedTypeSymbol prop)
            {
                var queryHandler = symbol.GetMembers().FirstOrDefault(x => x.Name == "QueryHandler");

                var queryHandlerProperties = new StringBuilder();
                var queryHandlerInjectedProperties = new StringBuilder();
                var queryHandlerConstructorParameters = new StringBuilder();
                var queryHandlerParameters = new StringBuilder();

                dynamic queryTypeArgument = "Unit";

                if (queryHandler is IMethodSymbol method)
                {
                    foreach (var parameter in method.Parameters)
                    {
                        if (parameter.Name.Equals("query", StringComparison.OrdinalIgnoreCase))
                        {
                            queryHandlerParameters.Append(
                                $"request{(SymbolEqualityComparer.Default.Equals(parameter, method.Parameters.Last()) ? "" : ", ")}");
                        }
                        else
                        {
                            queryHandlerProperties.AppendLine(
                                $"private readonly {parameter.Type} _{parameter.Name};");

                            queryHandlerParameters.Append(
                                $"_{parameter.Name}{(SymbolEqualityComparer.Default.Equals(parameter, method.Parameters.Last()) ? "" : ", ")}");

                            queryHandlerConstructorParameters.Append(
                                $"{parameter.Type} {parameter.Name}{(SymbolEqualityComparer.Default.Equals(parameter, method.Parameters.Last()) ? "" : ", ")}");

                            queryHandlerInjectedProperties.AppendLine($"_{parameter.Name} = {parameter.Name};");
                        }
                    }

                    var returnType = method.ReturnType as INamedTypeSymbol;
                    if (returnType is not null)
                    {
                        queryTypeArgument = returnType.TypeArguments.First();
                    }
                }

                var addValidation = prop.GetMembers().FirstOrDefault(x => x.Name == "AddValidation");

                var queryValidator = new StringBuilder();

                if (addValidation is not null)
                {
                    queryValidator.Append(
                        @$"private class QueryValidator : AbstractValidator<Query> {{ public QueryValidator() {{ Query.AddValidation(this); }} }}");
                }

                return @$"
public {(query.IsSealed ? "sealed" : "")} partial record Query : IRequest<{queryTypeArgument}> {{ }} 

{queryValidator}

public class _QueryHandler : IRequestHandler<Query, {queryTypeArgument}>
{{
    {queryHandlerProperties}

    public _QueryHandler({queryHandlerConstructorParameters})
    {{
        {queryHandlerInjectedProperties}
    }}

    public async Task<{queryTypeArgument}> Handle(Query request, CancellationToken cancellationToken)  
        => {(queryHandler is null ? $"await Task.FromResult(Unit.Value);" : $"await QueryHandler({queryHandlerParameters});")}
}}";
            }

            return "";
        }

        private static string GenerateCommandSource(INamedTypeSymbol symbol)
        {
            var command = symbol.GetMembers().FirstOrDefault(x => x.Name == "Command");
            if (command is INamedTypeSymbol prop)
            {
                var commandHandler = symbol.GetMembers().FirstOrDefault(x => x.Name == "CommandHandler");

                var commandHandlerProperties = new StringBuilder();
                var commandHandlerInjectedProperties = new StringBuilder();
                var commandHandlerConstructorParameters = new StringBuilder();
                var commandHandlerParameters = new StringBuilder();

                if (commandHandler is IMethodSymbol method)
                {
                    foreach (var parameter in method.Parameters)
                    {
                        if (parameter.Name.Equals("command", StringComparison.OrdinalIgnoreCase))
                        {
                            commandHandlerParameters.Append(
                                $"request{(SymbolEqualityComparer.Default.Equals(parameter, method.Parameters.Last()) ? "" : ", ")}");
                        }
                        else
                        {
                            commandHandlerProperties.AppendLine(
                                $"private readonly {parameter.Type} _{parameter.Name};");

                            commandHandlerParameters.Append(
                                $"_{parameter.Name}{(SymbolEqualityComparer.Default.Equals(parameter, method.Parameters.Last()) ? "" : ", ")}");

                            commandHandlerConstructorParameters.Append(
                                $"{parameter.Type} {parameter.Name}{(SymbolEqualityComparer.Default.Equals(parameter, method.Parameters.Last()) ? "" : ", ")}");

                            commandHandlerInjectedProperties.AppendLine($"_{parameter.Name} = {parameter.Name};");
                        }
                    }
                }

                var addValidation = prop.GetMembers().FirstOrDefault(x => x.Name == "AddValidation");

                var commandValidator = new StringBuilder();

                if (addValidation is not null)
                {
                    commandValidator.Append(
                        @$"private class CommandValidator : AbstractValidator<Command> {{ public CommandValidator() {{ Command.AddValidation(this); }} }}");
                }

                return @$"
public {(command.IsSealed ? "sealed" : "")} partial record Command : IRequest {{ }} 

{commandValidator}

public class _CommandHandler : AsyncRequestHandler<Command>
{{
    {commandHandlerProperties}

    public _CommandHandler({commandHandlerConstructorParameters})
    {{
        {commandHandlerInjectedProperties}
    }}

    protected override async Task Handle(Command request, CancellationToken cancellationToken)
        => {(commandHandler is null ? "await Task.CompletedTask;" : $"await CommandHandler({commandHandlerParameters});")}
}} 
";
            }

            return "";
        }
    }
}