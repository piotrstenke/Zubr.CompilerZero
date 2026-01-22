using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Linq;
using Zubr.Compiler.Syntax;
using Zubr.Compiler.Syntax.Abstractions;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Zubr.Compiler.CSharp;

internal sealed class CSharpTranslator
{
	public CSharpTranslatorOptions Options { get; }

	internal CSharpTranslator(CSharpTranslatorOptions options)
	{
		Options = options;
	}

	public static CSharpTranslator Create()
	{
		return Create(new());
	}

	public static CSharpTranslator Create(CSharpTranslatorOptions options)
	{
		return new(options);
	}

	public CSharpSyntaxTree Translate(SyntaxTree syntaxTree)
	{
		return (CSharpSyntaxTree)CSharpSyntaxTree.Create(Translate(syntaxTree.Root), encoding: syntaxTree.Encoding);
	}

	public Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax Translate(CompilationUnitSyntax node)
	{
		bool hasMultipleModules = node.Members.Count(x => x is ModuleDeclarationSyntax module) > 1;

		return CompilationUnit()
			.WithUsings(List(node.Uses.Select(x =>
			{
				if(x.Alias is null)
				{
					return UsingDirective(ToName(x.Name));
				}

				return UsingDirective(NameEquals(ToIdentifierName(x.Alias)), ToName(x.Name));
			})))
			.WithMembers(List<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax>(node.Members
				.Select(x => x switch
				{
					ModuleDeclarationSyntax module => ToNamespace(module, hasMultipleModules)!,
					_ => null!
				})
				.Where(x => x is not null)))
			.NormalizeWhitespace();
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax? ToNamespace(ModuleDeclarationSyntax node, bool useFileScoped)
	{
		if (node.Name is null)
		{
			// C# equivalent of 'module top;' is not declaring a namespace at all.
			return null;
		}

		if (useFileScoped)
		{
			return FileScopedNamespaceDeclaration(ToName(node.Name));
		}

		return NamespaceDeclaration(ToName(node.Name));
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax ToName(NameSyntax node)
	{
		return node switch
		{
			SimpleNameSyntax s => ToSimpleName(s),
			QualifiedNameSyntax q => QualifiedName(ToName(q.Left), ToSimpleName(q.Right)),

			_ => throw new UnreachableException()
		};
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.SimpleNameSyntax ToSimpleName(SimpleNameSyntax node)
	{
		return node switch
		{
			IdentifierNameSyntax i => ToIdentifierName(i),

			_ => throw new UnreachableException()
		};
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax ToIdentifierName(IdentifierNameSyntax node)
	{
		return IdentifierName(node.Identifier.Text);
	}
}
