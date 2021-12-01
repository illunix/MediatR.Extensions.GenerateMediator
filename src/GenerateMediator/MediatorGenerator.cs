using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateMediator
{
    [Generator]
    internal class MediatorGenerator : ISourceGenerator
    {
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
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            var attributeSymbol = context.Compilation.GetTypeByMetadataName("GenerateMediator.GenerateMediatorAttribute");
            var classSymbols = new List<INamedTypeSymbol>();

            foreach (var cls in receiver.CandidateClasses)
            {
                var model = context.Compilation.GetSemanticModel(cls.SyntaxTree);

                var classSymbol = model.GetDeclaredSymbol(cls);

                if (classSymbol.GetAttributes().Any(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)))
                {
                    classSymbols.Add(classSymbol);
                }
            }
            
            var sourceBuilder = new StringBuilder();

            var useFluentValidation = false;

            foreach (var symbol in classSymbols.Select(classSymbol => new List<ISymbol>
            {
                classSymbol.GetMembers().FirstOrDefault(x => x.Name == "Query"),
                classSymbol.GetMembers().FirstOrDefault(x => x.Name == "Command")
            }).SelectMany(symbols => symbols))
            {
                if (useFluentValidation is not false)
                {
                    continue;
                }

                if (symbol is INamedTypeSymbol prop)
                {
                    useFluentValidation = prop.GetMembers().Any(x => x.Name == "AddValidation");
                }
            } 
            
            sourceBuilder.Append(@$"
using MediatR;
{(useFluentValidation ? "using FluentValidation;" : "")}
using System.Threading;
using System.Threading.Tasks;");
            
            foreach (var classSymbol in classSymbols)
            {
                var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

                sourceBuilder.Append(@$"
namespace {namespaceName}
{{ 
    public {(classSymbol.IsStatic ? "static" : "")} partial class {classSymbol.Name} 
    {{       
        {GenerateQuerySource(classSymbol)}
        {GenerateCommandSource(classSymbol)}
    }} 
}}");
            }

            context.AddSource("GenerateMediator.g.cs",
                SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        private static string GenerateQuerySource(INamedTypeSymbol symbol)
        {
            var query = symbol.GetMembers().FirstOrDefault(x => x.Name == "Query");
            if (query is not INamedTypeSymbol prop)
            {
                return "";
            }
            
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
                    var emptyOrComma = SymbolEqualityComparer.Default.Equals(parameter, method.Parameters.Last()) ? "" : ", ";
                    
                    if (parameter.Name.Equals("query", StringComparison.OrdinalIgnoreCase))
                    {
                        queryHandlerParameters.Append(
                            $"request{emptyOrComma}");
                    }
                    else
                    {
                        queryHandlerProperties.AppendLine(
                            $"private readonly {parameter.Type} _{parameter.Name};");

                        queryHandlerParameters.Append(
                            $"_{parameter.Name}{emptyOrComma}");

                        queryHandlerConstructorParameters.Append(
                            $"{parameter.Type} {parameter.Name}{emptyOrComma}");

                        queryHandlerInjectedProperties.AppendLine($"_{parameter.Name} = {parameter.Name};");
                    }
                }

                if (method.ReturnType is INamedTypeSymbol returnType)
                {
                    queryTypeArgument = returnType.TypeArguments.First();
                }
            }

            var addValidation = prop.GetMembers().FirstOrDefault(x => x.Name == "AddValidation");
                
            var queryValidator = new StringBuilder();

            if (addValidation is not null)
            {
                queryValidator.Append(
                    @$"public class QueryValidator : AbstractValidator<Query> {{ public QueryValidator() {{ Query.AddValidation(this); }} }}");
            }

            return @$"
public {(query.IsSealed ? "sealed" : "")} partial record Query : IRequest<{queryTypeArgument}> {{ }} 

{queryValidator}

private class QueryHandlerCore : IRequestHandler<Query, {queryTypeArgument}>
{{
    {queryHandlerProperties}

    public QueryHandlerCore({queryHandlerConstructorParameters})
    {{
        {queryHandlerInjectedProperties}
    }}

    public async Task<{queryTypeArgument}> Handle(Query request, CancellationToken cancellationToken)  
        => {(queryHandler is null ? $"await Task.FromResult(Unit.Value);" : $"await QueryHandler({queryHandlerParameters});")}
}}";
        }

        private static string GenerateCommandSource(INamedTypeSymbol symbol)
        {
            var command = symbol.GetMembers().FirstOrDefault(x => x.Name == "Command");
            if (command is not INamedTypeSymbol prop)
            {
                return "";
            }
            
            var commandHandler = symbol.GetMembers().FirstOrDefault(x => x.Name == "CommandHandler");

            var commandHandlerProperties = new StringBuilder();
            var commandHandlerInjectedProperties = new StringBuilder();
            var commandHandlerConstructorParameters = new StringBuilder();
            var commandHandlerParameters = new StringBuilder();

            dynamic commandTypeArgument = null;

            if (commandHandler is IMethodSymbol method)
            {
                foreach (var parameter in method.Parameters)
                {
                    var emptyOrComma = SymbolEqualityComparer.Default.Equals(parameter, method.Parameters.Last()) ? "" : ", ";

                    if (parameter.Name.Equals("command", StringComparison.OrdinalIgnoreCase))
                    {
                        commandHandlerParameters.Append(
                            $"request{emptyOrComma}");
                    }
                    else
                    {
                        commandHandlerProperties.AppendLine(
                            $"private readonly {parameter.Type} _{parameter.Name};");

                        commandHandlerParameters.Append(
                            $"_{parameter.Name}{emptyOrComma}");

                        commandHandlerConstructorParameters.Append(
                            $"{parameter.Type} {parameter.Name}{emptyOrComma}");

                        commandHandlerInjectedProperties.AppendLine($"_{parameter.Name} = {parameter.Name};");
                    }
                }

                if (method.ReturnType is INamedTypeSymbol returnType)
                {
                    if (returnType.TypeParameters.Any())
                    {
                        commandTypeArgument = returnType.TypeArguments.First();
                    }
                }
            }

            var addValidation = prop.GetMembers().FirstOrDefault(x => x.Name == "AddValidation");

            var commandValidator = new StringBuilder();

            if (addValidation is not null)
            {
                commandValidator.Append(
                    @$"public class CommandValidator : AbstractValidator<Command> {{ public CommandValidator() {{ Command.AddValidation(this); }} }}");
            }

            if (commandTypeArgument is null)
            {
                return @$"
public {(command.IsSealed ? "sealed" : "")} partial record Command : IRequest {{ }} 

{commandValidator}

private class CommandHandlerCore : AsyncRequestHandler<Command>
{{
    {commandHandlerProperties}

    public CommandHandlerCore({commandHandlerConstructorParameters})
    {{
        {commandHandlerInjectedProperties}
    }}

    protected override async Task Handle(Command request, CancellationToken cancellationToken)
        => {(commandHandler is null ? "await Task.CompletedTask;" : $"await CommandHandler({commandHandlerParameters});")}
}} 
";
            }
            else
            {
                return @$"
public {(command.IsSealed ? "sealed" : "")} partial record Command : IRequest<{commandTypeArgument}> {{ }} 

{commandValidator}

private class CommandHandlerCore : IRequestHandler<Command, {commandTypeArgument}>
{{
    {commandHandlerProperties}

    public CommandHandlerCore({commandHandlerConstructorParameters})
    {{
        {commandHandlerInjectedProperties}
    }}

    public async Task<{commandTypeArgument}> Handle(Command request, CancellationToken cancellationToken)
        => {(commandHandler is null ? "await Task.FromResult(Unit.Value);" : $"await CommandHandler({commandHandlerParameters});")}
}} 
";
            }
        }
    }
}
