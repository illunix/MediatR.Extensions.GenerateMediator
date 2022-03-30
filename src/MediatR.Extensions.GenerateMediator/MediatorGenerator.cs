using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediatR.Extensions.GenerateMediator;

[Generator]
public class MediatorGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        Debugger.Launch();

        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
        {
            return;
        }

        var classes = GetClasses(
            context,
            receiver
        ).ToList();

        var sb = new StringBuilder();

        foreach (var clazz in classes)
        {
            sb.AppendLine(GetSource(clazz));
        }

        context.AddSource(
            "MediatR.Extensions.GenerateMediator.g.cs",
            SourceText.From(
                sb.ToString(),
                Encoding.UTF8
            )
        );
    }

    private static string GetSource(INamedTypeSymbol clazz)
    {
        #region Common
        var namespaceName = clazz.ContainingNamespace.ToDisplayString();

        var handlerMethod = clazz.GetMembers()
            .FirstOrDefault(q => q.Name == "Handler") as IMethodSymbol;
        var handlerMethodReturnType = (INamedTypeSymbol)handlerMethod.ReturnType;
        var handlerMethodParams = handlerMethod.Parameters
            .ToDictionary(q => q.Type, q => q.Name);
        var handlerMethodParamsWithoutRequest = handlerMethodParams.Where(q => q.Key.Name != "Command" && q.Key.Name != "Query").ToList();
        #endregion

        #region Properties
        var propertiesBuilder = new StringBuilder();

        foreach (var param in handlerMethodParams.Where(q => q.Key.Name != "Command" && q.Key.Name != "Query"))
        {
            propertiesBuilder.AppendLine($"private readonly {param.Key} _{param.Value};");
        }
        #endregion

        #region Request
        var requestBuilder = new StringBuilder();

        var commandExist = clazz.GetMembers()
            .Any(q => q.Name == "Command");

        var queryExist = clazz.GetMembers()
            .Any(q => q.Name == "Query");

        if (commandExist && queryExist)
        {
            return string.Empty;
        }

        dynamic type = null;

        if (handlerMethodReturnType.TypeArguments.Any())
        {
            type = handlerMethodReturnType.TypeArguments.First();
        }

        var requestInterface = type is not null ? $"IRequest<{handlerMethodReturnType.TypeArguments.FirstOrDefault()}>" : "IRequest";

        var requestMethodName = clazz.GetMembers().Any(q => q.Name == "Command") ? "Command" : "Query";

        requestBuilder.AppendLine($"public partial record {requestMethodName} : {requestInterface} {{ }}");
        #endregion

        #region Constructor
        var constructorBuilder = new StringBuilder();

        var constructorParams = string.Join(", ", handlerMethodParamsWithoutRequest.Select(q => $"{q.Key} {q.Value}"));
        var injected = string.Join("\n", handlerMethodParamsWithoutRequest.Select(q => $"_{q.Value} = {q.Value};"));

        constructorBuilder.Append($@"
public {clazz.Name}({constructorParams})
{{
    {injected}
}}
");
        #endregion

        #region Handle
        var handleBuilder = new StringBuilder();

        handleBuilder.AppendLine(@$"
public async Task<{(type is null ? "Unit": type)}> Handle({requestMethodName} request, CancellationToken cancellationToken) 
{{
    {(type is null ? "return Unit.Value;" : 
        $"return await Handler({string.Join(", ", handlerMethodParams.Values.Select(q => q == "request" || q == "command" || q == "query" ? "request" : $"_{q}"))});")}
}}
");
        #endregion

        return @$"
namespace {namespaceName}
{{
    public partial class {clazz.Name} : IRequestHandler<{clazz.Name}.{requestMethodName}{(type is null ? "" : $", {type}")}>
    {{
        {propertiesBuilder.ToString()}

        {requestBuilder.ToString()}

        {constructorBuilder.ToString()}

        {handleBuilder.ToString()}
    }}
}}
";
    }

    private static IEnumerable<INamedTypeSymbol> GetClasses(
        GeneratorExecutionContext context,
        SyntaxReceiver receiver
    )
    {
        var compilation = context.Compilation;

        foreach (var clazz in receiver.CandidateClasses)
        {
            var model = compilation.GetSemanticModel(clazz.SyntaxTree);
            var classSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(clazz);
            if (classSymbol is null)
            {
                break;
            }

            if (classSymbol.GetAttributes().Any(q => q.AttributeClass?.Name == nameof(GenerateMediatorAttribute)))
            {
                yield return classSymbol;
            }
        }
    }
}