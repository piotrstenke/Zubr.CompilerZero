using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Diagnostics;
using Zubr.Compiler.Syntax;
using Zubr.Compiler.Syntax.Abstractions;

using CSyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

using Sharp = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Zubr.Compiler.CSharp;

internal sealed partial class CSharpTranslator
{
	private static class Expressions
	{
		public static Sharp.ExpressionSyntax Expression(ExpressionSyntax node)
		{
			return node switch
			{
				TypeSyntax t => Type(t),
				LiteralExpressionSyntax l => Literal(l),
				_ => throw new UnreachableException()
			};
		}

		public static Sharp.LiteralExpressionSyntax Literal(LiteralExpressionSyntax node)
		{
			return node.Kind switch
			{
				SyntaxKind.TrueLiteralExpression
					=> SyntaxFactory.LiteralExpression(CSyntaxKind.TrueLiteralExpression),

				SyntaxKind.FalseLiteralExpression
					=> SyntaxFactory.LiteralExpression(CSyntaxKind.FalseLiteralExpression),

				SyntaxKind.NumericLiteralExpression
					=> NumericLiteral(node),

				SyntaxKind.StringLiteralExpression
					=> SyntaxFactory.LiteralExpression(CSyntaxKind.StringLiteralExpression,
						SyntaxFactory.Literal(node.Value.Text, node.Value.Text.Trim('\"'))),

				SyntaxKind.CharLiteralExpression
					=> SyntaxFactory.LiteralExpression(CSyntaxKind.CharacterLiteralExpression, 
						SyntaxFactory.Literal(node.Value.Text, char.Parse(node.Value.Text.Trim('\'')))),

				_ => throw new UnreachableException()
			};
		}

		public static Sharp.TypeSyntax Type(TypeSyntax node)
		{
			return node switch
			{
				NameSyntax n => Name(n),
				PredefinedTypeSyntax p => PredefinedType(p),
				_ => throw new UnreachableException()
			};
		}

		public static Sharp.PredefinedTypeSyntax PredefinedType(PredefinedTypeSyntax node)
		{
			return node.Keyword.Kind switch
			{
				SyntaxKind.IntKeyword => SyntaxFactory.PredefinedType(Token(CSyntaxKind.IntKeyword)),
				SyntaxKind.StringKeyword => SyntaxFactory.PredefinedType(Token(CSyntaxKind.StringKeyword)),
				SyntaxKind.BoolKeyword => SyntaxFactory.PredefinedType(Token(CSyntaxKind.BoolKeyword)),
				SyntaxKind.VoidKeyword => SyntaxFactory.PredefinedType(Token(CSyntaxKind.VoidKeyword)),
				_ => throw new UnreachableException()
			};
		}

		public static Sharp.NameSyntax Name(NameSyntax node)
		{
			return node switch
			{
				SimpleNameSyntax s => SimpleName(s),
				QualifiedNameSyntax q => SyntaxFactory.QualifiedName(Name(q.Left), SimpleName(q.Right)),

				_ => throw new UnreachableException()
			};
		}

		public static Sharp.SimpleNameSyntax SimpleName(SimpleNameSyntax node)
		{
			return node switch
			{
				IdentifierNameSyntax i => IdentifierName(i),

				_ => throw new UnreachableException()
			};
		}

		public static Sharp.IdentifierNameSyntax IdentifierName(IdentifierNameSyntax node)
		{
			return SyntaxFactory.IdentifierName(node.Identifier.Text);
		}

		private static Sharp.LiteralExpressionSyntax NumericLiteral(LiteralExpressionSyntax node)
		{
			SyntaxToken current = node.Value;

			Microsoft.CodeAnalysis.SyntaxToken token = current.Value switch
			{
				int @int => SyntaxFactory.Literal(default, current.Text, @int, default),
				long @long => SyntaxFactory.Literal(default, current.Text, @long, default),
				short @short => SyntaxFactory.Literal(default, current.Text, @short, default),
				byte @byte => SyntaxFactory.Literal(default, current.Text, @byte, default),
				uint @uint => SyntaxFactory.Literal(default, current.Text, @uint, default),
				ulong @ulong => SyntaxFactory.Literal(default, current.Text, @ulong, default),
				ushort @ushort => SyntaxFactory.Literal(default, current.Text, @ushort, default),
				sbyte @sbyte => SyntaxFactory.Literal(default, current.Text, @sbyte, default),
				float @float => SyntaxFactory.Literal(default, current.Text, @float, default),
				double @double => SyntaxFactory.Literal(default, current.Text, @double, default),
				decimal @decimal => SyntaxFactory.Literal(default, current.Text, @decimal, default),
				_ => SyntaxFactory.Literal(default, current.Text, 0, default)
			};

			return SyntaxFactory.LiteralExpression(CSyntaxKind.NumericLiteralExpression, token);
		}
	}
}
