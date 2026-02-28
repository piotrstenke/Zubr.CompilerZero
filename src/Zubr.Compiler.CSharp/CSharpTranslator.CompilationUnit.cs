using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using Zubr.Compiler.Syntax;
using Zubr.Compiler.Syntax.Abstractions;

using CSyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using Sharp = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Zubr.Compiler.CSharp;

internal sealed partial class CSharpTranslator
{
	private static Sharp.CompilationUnitSyntax Translate(CompilationUnitSyntax node)
	{
		return SyntaxFactory.CompilationUnit()
			.WithUsings(SyntaxFactory.List(node.Uses.Select(UsingDirective)))
			.WithMembers(SyntaxFactory.List(GetCompilationUnitMembers(node)))
			.NormalizeWhitespace();
	}

	private static Sharp.UsingDirectiveSyntax UsingDirective(UseDirectiveSyntax x)
	{
		if (x.Alias is null)
		{
			return SyntaxFactory.UsingDirective(Expressions.Name(x.Name));
		}

		if (x.ModuleName is null)
		{
			return SyntaxFactory.UsingDirective(SyntaxFactory.NameEquals(Expressions.IdentifierName(x.Alias)), Expressions.Name(x.Name));
		}

		return SyntaxFactory.UsingDirective(
			SyntaxFactory.NameEquals(Expressions.IdentifierName(x.Alias)),
			SyntaxFactory.QualifiedName(Expressions.Name(x.ModuleName), Expressions.SimpleName((SimpleNameSyntax)x.Name)) // TODO: Support non-simple names.
		);
	}

	private static List<Sharp.MemberDeclarationSyntax> GetCompilationUnitMembers(CompilationUnitSyntax node)
	{
		bool hasMultipleModules = HasMultipleModules(node);

		List<Sharp.MemberDeclarationSyntax> members = new(node.Members.Count);

		List<Sharp.MethodDeclarationSyntax> globalFunctions = new();

		TypeFlags typeFlags = default;

		foreach (MemberDeclarationSyntax member in node.Members)
		{
			switch (member)
			{
				case ModuleDeclarationSyntax module:

					if (Declarations.Namespace(module, hasMultipleModules) is Sharp.BaseNamespaceDeclarationSyntax @namespace)
					{
						members.Add(@namespace);
					}
					else
					{
						AddModuleMembers(module, members, globalFunctions);
					}

					break;

				case FunctionDeclarationSyntax func:
					// Top-level functions are added later inside of a partial class.
					globalFunctions.Add(Declarations.Method(func));
					break;

				default:
					members.Add(Declarations.Member(member, ref typeFlags));
					break;
			}
		}

		if (globalFunctions.Count > 0)
		{
			members.Add(GlobalFunctionContainerType(globalFunctions));
		}

		return members;

		static bool HasMultipleModules(CompilationUnitSyntax node)
		{
			var members = node.Members;

			bool foundModule = false;

			for (int i = 0; i < members.Count; i++)
			{
				if (members[i] is ModuleDeclarationSyntax)
				{
					if (foundModule)
					{
						return true;
					}

					foundModule = true;
				}
			}

			return false;
		}
	}

	private static Sharp.ClassDeclarationSyntax GlobalFunctionContainerType(List<Sharp.MethodDeclarationSyntax> globalFunctions)
	{
		return SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier("TopLevel"))
			.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(CSyntaxKind.StaticKeyword), SyntaxFactory.Token(CSyntaxKind.PartialKeyword)))
			.WithMembers(SyntaxFactory.List<Sharp.MemberDeclarationSyntax>(globalFunctions));
	}

	private static void AddModuleMembers(
		ModuleDeclarationSyntax node,
		List<Sharp.MemberDeclarationSyntax> members,
		List<Sharp.MethodDeclarationSyntax> globalFunctions
	)
	{
		TypeFlags typeFlags = default;

		foreach (MemberDeclarationSyntax member in node.Members)
		{
			if (member is FunctionDeclarationSyntax func)
			{
				globalFunctions.Add(Declarations.Method(func));
			}
			else
			{
				members.Add(Declarations.Member(member, ref typeFlags));
			}
		}
	}
}
