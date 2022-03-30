using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediatR.Extensions.GenerateMediator;

public class SyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
            classDeclarationSyntax.AttributeLists.Count > 0)
        {
            CandidateClasses.Add(classDeclarationSyntax);
        }
    }
}