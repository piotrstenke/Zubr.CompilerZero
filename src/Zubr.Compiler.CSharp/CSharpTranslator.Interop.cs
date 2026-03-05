using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Zubr.Compiler.Syntax.Abstractions;
using CSyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

using Sharp = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Zubr.Compiler.CSharp;

partial class CSharpTranslator
{
	private static class Interop
	{
		public static Sharp.AttributeListSyntax InternalInheritAttribute()
		{
			return Attribute("zubr", "interop", "csharp", "InternalInherit");
		}

		public static Sharp.AttributeListSyntax MustOverrideAttribute()
		{
			return Attribute("zubr", "interop", "csharp", "MustOverride");
		}

		public static Sharp.AttributeListSyntax FlagsAttribute()
		{
			return Attribute("System", "Flags");
		}

		public static Sharp.AttributeListSyntax InvokerAttribute()
		{
			return Attribute("zubr", "interop", "csharp", "Invoker");
		}

		public static Sharp.AttributeListSyntax DefaultTypeParameterAttribute(TypeSyntax type)
		{
			return AttributeWithArgs([SyntaxFactory.TypeOfExpression(Expressions.Type(type))], "zubr", "interop", "csharp", "DefaultTypeParameter");
		}

		public static Sharp.SimpleBaseTypeSyntax ImplementIDisposable()
		{
			return SyntaxFactory.SimpleBaseType(Expressions.GlobalQualifiedName("System", "IDisposable"));
		}

		public static IEnumerable<Sharp.MemberDeclarationSyntax> DisposablePattern(bool isOpen, bool unmanaged)
		{
			yield return SyntaxFactory.ParseMemberDeclaration(
@"private bool _disposed;")!;

			yield return SyntaxFactory.ParseMemberDeclaration(
@"public void free()
{
	free(true);
	global::System.GC.SuppressFinalize(this);
}")!;

			yield return SyntaxFactory.ParseMemberDeclaration(
@"void global::System.IDisposable.Dispose()
{
	free();
}")!;

			string modifiers = isOpen
				? "protected virtual"
				: "private";

			if(unmanaged)
			{
				yield return SyntaxFactory.ParseMemberDeclaration(
modifiers + @" void free(bool disposing)
{
	if(_disposed)
	{
		return;
	}

	if(disposing)
	{
		free_managed();
	}

	free_unmanaged();

	_disposed = true;
}")!;
			}
			else
			{
				yield return SyntaxFactory.ParseMemberDeclaration(
modifiers + @" void free(bool disposing)
{
	if(_disposed)
	{
		return;
	}

	if(disposing)
	{
		free_managed();
	}

	_disposed = true;
}")!;
			}
		}

		public static Sharp.DestructorDeclarationSyntax IDisposableDestructor(SyntaxToken identifier)
		{
			return SyntaxFactory.DestructorDeclaration(
				default,
				default,
				identifier,
				SyntaxFactory.ParameterList(),
				SyntaxFactory.Block(SyntaxFactory.SingletonList<Sharp.StatementSyntax>(
					SyntaxFactory.ExpressionStatement(
						SyntaxFactory.InvocationExpression(
							SyntaxFactory.IdentifierName("free"),
							SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
								SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(CSyntaxKind.FalseLiteralExpression)))))))));
		}

		private static Sharp.AttributeListSyntax Attribute(params ReadOnlySpan<string> names)
		{
			return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(Expressions.GlobalQualifiedName(names))));
		}

		private static Sharp.AttributeListSyntax AttributeWithArgs(Sharp.ExpressionSyntax[] args, params ReadOnlySpan<string> names)
		{
			return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(
				Expressions.GlobalQualifiedName(names),
				SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(args.Select(SyntaxFactory.AttributeArgument))))));
		}
	}
}
