using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Zubr.Compiler.Syntax;
using Zubr.Compiler.Syntax.Abstractions;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

using Sharp = Microsoft.CodeAnalysis.CSharp.Syntax;

using CSyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Zubr.Compiler.CSharp;

internal sealed partial class CSharpTranslator
{
	private Sharp.CompilationUnitSyntax Translate(CompilationUnitSyntax node)
	{
		bool hasMultipleModules = node.Members.Count(x => x is ModuleDeclarationSyntax module) > 1;

		return CompilationUnit()
			.WithUsings(List(node.Uses.Select(x =>
			{
				if (x.Alias is null)
				{
					return UsingDirective(Expressions.Name(x.Name));
				}

				return UsingDirective(NameEquals(Expressions.IdentifierName(x.Alias)), Expressions.Name(x.Name));
			})))
			.WithMembers(List(GetCompilationUnitMembers(node)))
			.NormalizeWhitespace();
	}

	private static List<Sharp.MemberDeclarationSyntax> GetCompilationUnitMembers(CompilationUnitSyntax node)
	{
		bool hasMultipleModules = HasMultipleModules(node);

		List<Sharp.MemberDeclarationSyntax> members = new(node.Members.Count);

		List<Sharp.MethodDeclarationSyntax> globalFunctions = new();

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

				case ClassDeclarationSyntax @class:
					members.Add(Declarations.Class(@class));
					break;

				case StructDeclarationSyntax @struct:
					members.Add(Declarations.Struct(@struct));
					break;

				case FunctionDeclarationSyntax func:
					// Top-level functions are added later inside of a partial class.
					globalFunctions.Add(Declarations.Method(func));
					continue;

				default:
					continue;
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
		return ClassDeclaration(Identifier("TopLevel"))
			.WithModifiers(TokenList(Token(CSyntaxKind.StaticKeyword), Token(CSyntaxKind.PartialKeyword)))
			.WithMembers(List<Sharp.MemberDeclarationSyntax>(globalFunctions));
	}

	private static void AddModuleMembers(
		ModuleDeclarationSyntax node,
		List<Sharp.MemberDeclarationSyntax> members,
		List<Sharp.MethodDeclarationSyntax> globalFunctions
	)
	{
		foreach (MemberDeclarationSyntax member in node.Members)
		{
			if (member is FunctionDeclarationSyntax func)
			{
				globalFunctions.Add(Declarations.Method(func));
			}
			else
			{
				members.Add(Declarations.Member(member));
			}
		}
	}
}
