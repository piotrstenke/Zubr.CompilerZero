using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Zubr.Compiler.Syntax;
using Zubr.Compiler.Syntax.Abstractions;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

using Sharp = Microsoft.CodeAnalysis.CSharp.Syntax;

using CSyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using CSyntaxToken = Microsoft.CodeAnalysis.SyntaxToken;
using CSyntaxTokenList = Microsoft.CodeAnalysis.SyntaxTokenList;

using SyntaxTokenList = Zubr.Compiler.Syntax.Abstractions.SyntaxTokenList;

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

	public Sharp.CompilationUnitSyntax Translate(CompilationUnitSyntax node)
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
			switch(member)
			{
				case ModuleDeclarationSyntax module:

					if(ToNamespace(module, hasMultipleModules) is Sharp.BaseNamespaceDeclarationSyntax @namespace)
					{
						members.Add(@namespace);
					}
					else
					{
						AddModuleMembers(module, members, globalFunctions);
					}

					break;

				case ClassDeclarationSyntax @class:
					members.Add(ToClassDeclaration(@class));
					break;

				case StructDeclarationSyntax @struct:
					members.Add(ToStructDeclaration(@struct));
					break;

				case FunctionDeclarationSyntax func:
					// Top-level functions are added later inside of a partial class.
					globalFunctions.Add(ToMethodDeclaration(func));
					continue;

				default:
					continue;
			}
		}

		if(globalFunctions.Count > 0)
		{
			members.Add(ToGlobalFunctionContainerType(globalFunctions));
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

	private static Sharp.ClassDeclarationSyntax ToGlobalFunctionContainerType(List<Sharp.MethodDeclarationSyntax> globalFunctions)
	{
		return ClassDeclaration(Identifier("TopLevel"))
			.WithModifiers(TokenList(Token(CSyntaxKind.StaticKeyword), Token(CSyntaxKind.PartialKeyword)))
			.WithMembers(List<Sharp.MemberDeclarationSyntax>(globalFunctions));
	}

	private static Sharp.MethodDeclarationSyntax ToMethodDeclaration(FunctionDeclarationSyntax node)
	{
		Sharp.MethodDeclarationSyntax method = MethodDeclaration(ToType(node.ReturnType), Identifier(node.Identifier.Text))
			.WithBody(Block());

		var parameters = node.ParameterList.Parameters;

		if (parameters.Count > 0)
		{
			method = method.WithParameterList(ParameterList(SeparatedList(node.ParameterList.Parameters.Select(ToParameter))));
		}

		return method;
	}

	private static Sharp.ParameterSyntax ToParameter(ParameterSyntax x)
	{
		Sharp.EqualsValueClauseSyntax? defaultClause = x.Default is null
			? null
			: EqualsValueClause(ToExpression(x.Default.Value));

		return Parameter(default, default, ToType(x.Type), Identifier(x.Identifier.Text), defaultClause);
	}

	private static Sharp.ClassDeclarationSyntax ToClassDeclaration(ClassDeclarationSyntax node)
	{
		CSyntaxTokenList modifiers = GetModifiers(node);

		return ClassDeclaration(node.Identifier.Text)
			.WithModifiers(modifiers);
	}

	private static Sharp.StructDeclarationSyntax ToStructDeclaration(StructDeclarationSyntax node)
	{
		CSyntaxTokenList modifiers = GetModifiers(node);

		return StructDeclaration(node.Identifier.Text)
			.WithModifiers(modifiers);
	}

	private static CSyntaxTokenList GetModifiers(MemberDeclarationSyntax node)
	{
		SyntaxTokenList currentModifiers = node.Modifiers;
		List<CSyntaxToken> targetModifiers = new(currentModifiers.Count);

		bool isOpen = false;
		bool changePrivateToInternal = false;

		for (int i = 0; i < currentModifiers.Count; i++)
		{
			if (currentModifiers[i].IsKind(SyntaxKind.OpenKeyword))
			{
				isOpen = true;
				continue;
			}

			if (currentModifiers[i].IsKind(SyntaxKind.PrivKeyword) && node.Parent is not TypeDeclarationSyntax)
			{
				changePrivateToInternal = true;
				continue;
			}

			CSyntaxKind modifierKind = GetAccessModifierKind(currentModifiers[i].Kind);

			if (modifierKind == CSyntaxKind.None)
			{
				continue;
			}

			targetModifiers.Add(Token(modifierKind));
		}

		if(changePrivateToInternal && targetModifiers.Count == 0)
		{
			targetModifiers.Add(Token(CSyntaxKind.InternalKeyword));
		}

		if(!isOpen && node is ClassDeclarationSyntax)
		{
			targetModifiers.Add(Token(CSyntaxKind.SealedKeyword));
		}

		return TokenList(targetModifiers.ToArray());
	}

	private static CSyntaxKind GetAccessModifierKind(SyntaxKind value)
	{
		return value switch
		{
			SyntaxKind.PubKeyword => CSyntaxKind.PublicKeyword,
			SyntaxKind.ProtKeyword => CSyntaxKind.ProtectedKeyword,
			SyntaxKind.PrivKeyword => CSyntaxKind.PrivateKeyword,
			SyntaxKind.ScopedKeyword => CSyntaxKind.InternalKeyword,
			_ => default
		};
	}

	private static Sharp.BaseNamespaceDeclarationSyntax? ToNamespace(ModuleDeclarationSyntax node, bool hasMultipleModules)
	{
		if (node.Name is null)
		{
			// C# equivalent of 'module top;' is not declaring a namespace at all.
			return null;
		}

		Sharp.BaseNamespaceDeclarationSyntax @namespace = hasMultipleModules
			? NamespaceDeclaration(ToName(node.Name!))
			: FileScopedNamespaceDeclaration(ToName(node.Name!));

		List<Sharp.MemberDeclarationSyntax> targetMembers = new(node.Members.Count);
		List<Sharp.MethodDeclarationSyntax> globalFunctions = new();

		AddModuleMembers(node, targetMembers, globalFunctions);

		if (targetMembers.Count > 0)
		{
			@namespace = @namespace.WithMembers(List(targetMembers));
		}

		if(globalFunctions.Count > 0)
		{
			@namespace = @namespace.AddMembers(ToGlobalFunctionContainerType(globalFunctions));
		}

		return @namespace;
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
				globalFunctions.Add(ToMethodDeclaration(func));
			}
			else
			{
				members.Add(member switch
				{
					ClassDeclarationSyntax c => ToClassDeclaration(c),
					StructDeclarationSyntax s => ToStructDeclaration(s),
					_ => throw new UnreachableException()
				});
			}
		}
	}

	private static Sharp.BlockSyntax ToBlock(BlockSyntax node)
	{
		return Block(node.Statements.Select(ToStatement));
	}

	private static Sharp.StatementSyntax ToStatement(StatementSyntax node)
	{
		return node switch
		{
			BlockSyntax b => ToBlock(b),
			_ => throw new UnreachableException()
		};
	}

	private static Sharp.ExpressionSyntax ToExpression(ExpressionSyntax node)
	{
		return node switch
		{
			TypeSyntax t => ToType(t),
			LiteralExpressionSyntax l => ToLiteralExpression(l),
			_ => throw new UnreachableException()
		};
	}

	private static Sharp.LiteralExpressionSyntax ToLiteralExpression(LiteralExpressionSyntax node)
	{
		return node.Kind switch
		{
			SyntaxKind.TrueLiteralExpression => LiteralExpression(CSyntaxKind.TrueLiteralExpression),
			SyntaxKind.FalseLiteralExpression => LiteralExpression(CSyntaxKind.FalseLiteralExpression),
			SyntaxKind.NumericLiteralExpression => LiteralExpression(CSyntaxKind.NumericLiteralExpression, Token(CSyntaxKind.NumericLiteralToken, node.Value)),
			SyntaxKind.StringLiteralExpression => LiteralExpression(CSyntaxKind.StringLiteralExpression, Token(CSyntaxKind.StringLiteralToken, node.Value)),
			SyntaxKind.CharLiteralExpression => LiteralExpression(CSyntaxKind.CharacterLiteralExpression, Token(CSyntaxKind.CharacterLiteralToken, node.Value)),
			_ => throw new UnreachableException()
		};
	}

	private static Sharp.TypeSyntax ToType(TypeSyntax node)
	{
		return node switch
		{
			NameSyntax n => ToName(n),
			PredefinedTypeSyntax p => ToPredefinedType(p),
			_ => throw new UnreachableException()
		};
	}

	private static Sharp.PredefinedTypeSyntax ToPredefinedType(PredefinedTypeSyntax node)
	{
		return node.Keyword.Kind switch
		{
			SyntaxKind.IntKeyword => PredefinedType(Token(CSyntaxKind.IntKeyword)),
			SyntaxKind.StringKeyword => PredefinedType(Token(CSyntaxKind.StringKeyword)),
			SyntaxKind.BoolKeyword => PredefinedType(Token(CSyntaxKind.BoolKeyword)),
			SyntaxKind.VoidKeyword => PredefinedType(Token(CSyntaxKind.VoidKeyword)),
			_ => throw new UnreachableException()
		};
	}

	private static Sharp.NameSyntax ToName(NameSyntax node)
	{
		return node switch
		{
			SimpleNameSyntax s => ToSimpleName(s),
			QualifiedNameSyntax q => QualifiedName(ToName(q.Left), ToSimpleName(q.Right)),

			_ => throw new UnreachableException()
		};
	}

	private static Sharp.SimpleNameSyntax ToSimpleName(SimpleNameSyntax node)
	{
		return node switch
		{
			IdentifierNameSyntax i => ToIdentifierName(i),

			_ => throw new UnreachableException()
		};
	}

	private static Sharp.IdentifierNameSyntax ToIdentifierName(IdentifierNameSyntax node)
	{
		return IdentifierName(node.Identifier.Text);
	}

	private static CSyntaxToken Token(CSyntaxKind kind)
	{
		return SyntaxFactory.Token(kind);
	}

	private static CSyntaxToken Token(CSyntaxKind kind, SyntaxToken token)
	{
		return SyntaxFactory.Token(default, kind, token.Text, token.Text, default);
	}
}
