using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Zubr.Compiler.Syntax;
using Zubr.Compiler.Syntax.Abstractions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using CSyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using CSyntaxToken = Microsoft.CodeAnalysis.SyntaxToken;
using CSyntaxTokenList = Microsoft.CodeAnalysis.SyntaxTokenList;
using Sharp = Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxTokenList = Zubr.Compiler.Syntax.Abstractions.SyntaxTokenList;

namespace Zubr.Compiler.CSharp;

internal sealed partial class CSharpTranslator
{
	private static class Declarations
	{
		public static Sharp.BaseNamespaceDeclarationSyntax? Namespace(ModuleDeclarationSyntax node, bool hasMultipleModules)
		{
			if (node.Name is null)
			{
				// C# equivalent of 'module top;' is not declaring a namespace at all.
				return null;
			}

			Sharp.BaseNamespaceDeclarationSyntax @namespace = hasMultipleModules
				? NamespaceDeclaration(Expressions.Name(node.Name!))
				: FileScopedNamespaceDeclaration(Expressions.Name(node.Name!));

			List<Sharp.MemberDeclarationSyntax> targetMembers = new(node.Members.Count);
			List<Sharp.MethodDeclarationSyntax> globalFunctions = new();

			AddModuleMembers(node, targetMembers, globalFunctions);

			if (targetMembers.Count > 0)
			{
				@namespace = @namespace.WithMembers(List(targetMembers));
			}

			if (globalFunctions.Count > 0)
			{
				@namespace = @namespace.AddMembers(GlobalFunctionContainerType(globalFunctions));
			}

			return @namespace;
		}

		public static Sharp.MemberDeclarationSyntax Member(MemberDeclarationSyntax node)
		{
			return node switch
			{
				ClassDeclarationSyntax c => Class(c),
				StructDeclarationSyntax s => Struct(s),
				FunctionDeclarationSyntax f => Method(f),
				_ => throw new UnreachableException()
			};
		}

		public static Sharp.ClassDeclarationSyntax Class(ClassDeclarationSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node);
			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			var constraints = ConstraintList(node.ConstraintList);

			return ClassDeclaration(
				default,
				modifiers,
				Identifier(node.Identifier.Text),
				typeParameterList,
				default,
				constraints,
				List(node.Members.Select(Member))
			);
		}

		public static Sharp.StructDeclarationSyntax Struct(StructDeclarationSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node);
			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			var constraints = ConstraintList(node.ConstraintList);

			return StructDeclaration(
				default,
				modifiers,
				Identifier(node.Identifier.Text),
				typeParameterList,
				default,
				constraints,
				List(node.Members.Select(Member))
			);
		}

		public static Sharp.MethodDeclarationSyntax Method(FunctionDeclarationSyntax node)
		{
			Sharp.TypeSyntax returnType = Expressions.Type(node.ReturnType);
			CSyntaxTokenList modifiers = GetModifiers(node);
			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			var parameters = ParameterList(SeparatedList(node.ParameterList.Parameters.Select(Parameter)));
			var constraints = ConstraintList(node.ConstraintList);

			return MethodDeclaration(
				default,
				modifiers,
				returnType,
				default,
				Identifier(node.Identifier.Text),
				typeParameterList,
				parameters,
				constraints,
				Statements.Block(node.Body),
				default,
				default
			);
		}

		private static Sharp.ParameterSyntax Parameter(ParameterSyntax x)
		{
			Sharp.EqualsValueClauseSyntax? defaultClause = x.Default is null
				? null
				: EqualsValueClause(Expressions.Expression(x.Default.Value));

			return SyntaxFactory.Parameter(
				default,
				default,
				Expressions.Type(x.Type),
				Identifier(x.Identifier.Text),
				defaultClause
			);
		}

		private static Sharp.TypeParameterListSyntax? TypeParameterList(TypeParameterListSyntax? node)
		{
			if (node is null)
			{
				return null;
			}

			return SyntaxFactory.TypeParameterList(SeparatedList(node.Parameters.Select(TypeParameter)));
		}

		private static Sharp.TypeParameterSyntax TypeParameter(TypeParameterSyntax node)
		{
			return SyntaxFactory.TypeParameter(Identifier(node.Identifier.Text));
		}

		private static Microsoft.CodeAnalysis.SyntaxList<Sharp.TypeParameterConstraintClauseSyntax> ConstraintList(TypeParameterConstraintListSyntax? node)
		{
			if (node is null)
			{
				return default;
			}

			return List(node.Clauses
				.Select(x => TypeParameterConstraintClause(
					IdentifierName(Identifier(x.Identifier.Text)),
					SeparatedList(x.Constraints
						.Select<TypeParameterConstraintSyntax, Sharp.TypeParameterConstraintSyntax>(x => x switch
						{
							ClassConstraintSyntax c => ClassOrStructConstraint(CSyntaxKind.ClassConstraint),
							StructConstraintSyntax s => ClassOrStructConstraint(CSyntaxKind.StructConstraint),
							TypeConstraintSyntax t => TypeConstraint(Expressions.Type(t.Type)),
							_ => throw new UnreachableException()
						})
						// class and struct constraints must be defined first in C#
						.OrderBy(x => x is Sharp.ClassOrStructConstraintSyntax ? 0 : 1)
					)
				))
			);
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

			if (changePrivateToInternal && targetModifiers.Count == 0)
			{
				targetModifiers.Add(Token(CSyntaxKind.InternalKeyword));
			}

			if (!isOpen && node is ClassDeclarationSyntax)
			{
				targetModifiers.Add(Token(CSyntaxKind.SealedKeyword));
			}

			if (node is FunctionDeclarationSyntax && node.Parent is CompilationUnitSyntax or ModuleDeclarationSyntax)
			{
				if (!targetModifiers.Any(x => Microsoft.CodeAnalysis.CSharp.SyntaxFacts.IsAccessibilityModifier(x.Kind())))
				{
					targetModifiers.Add(Token(CSyntaxKind.PublicKeyword));
				}

				targetModifiers.Add(Token(CSyntaxKind.StaticKeyword));
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
	}
}
