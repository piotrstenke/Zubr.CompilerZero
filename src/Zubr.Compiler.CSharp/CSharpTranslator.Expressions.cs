using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Linq;
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
				BinaryExpressionSyntax b => Binary(b),
				PostfixUnaryExpression postfix => PostfixUnary(postfix),
				PrefixUnaryExpression prefix => PrefixUnary(prefix),
				MemberAccessExpressionSyntax m => MemberAccess(m),
				SelfExpressionSyntax s => This(s),
				CastExpressionSyntax c => Cast(c),
				InvocationExpressionSyntax i => Invocation(i),
				ParenthesizedExpressionSyntax p => Parenthesized(p),
				ConditionalExpressionSyntax conditional => Conditional(conditional),
				AssignmentExpressionSyntax a => Assignment(a),
				_ => throw new UnreachableException()
			};
		}

		public static Sharp.CastExpressionSyntax Cast(CastExpressionSyntax node)
		{
			return SyntaxFactory.CastExpression(Type(node.Type), Expression(node.Expression));
		}

		public static Sharp.ConditionalExpressionSyntax Conditional(ConditionalExpressionSyntax node)
		{
			return SyntaxFactory.ConditionalExpression(
				Expression(node.Condition),
				Expression(node.TrueExpression),
				Expression(node.FalseExpression)
			);
		}

		public static Sharp.ParenthesizedExpressionSyntax Parenthesized(ParenthesizedExpressionSyntax node)
		{
			return SyntaxFactory.ParenthesizedExpression(Expression(node.Expression));
		}

		public static Sharp.ThisExpressionSyntax This(SelfExpressionSyntax node)
		{
			return SyntaxFactory.ThisExpression();
		}

		public static Sharp.InvocationExpressionSyntax Invocation(InvocationExpressionSyntax node)
		{
			return SyntaxFactory.InvocationExpression(
				Expression(node.Expression),
				SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
					node.ArgumentList.Arguments.Select(x => SyntaxFactory.Argument(Expression(x.Expression)))))
			);
		}

		public static Sharp.MemberAccessExpressionSyntax MemberAccess(MemberAccessExpressionSyntax node)
		{
			return SyntaxFactory.MemberAccessExpression(CSyntaxKind.SimpleMemberAccessExpression, Expression(node.Expression), SimpleName(node.Name));
		}

		public static Sharp.AssignmentExpressionSyntax Assignment(AssignmentExpressionSyntax node)
		{
			CSyntaxKind kind = GetAssignmentKind(node.Kind);
			return SyntaxFactory.AssignmentExpression(kind, Expression(node.Left), Expression(node.Right));
		}

		public static Sharp.PostfixUnaryExpressionSyntax PostfixUnary(PostfixUnaryExpression node)
		{
			CSyntaxKind kind = GetPostfixUnaryKind(node.Kind);
			return SyntaxFactory.PostfixUnaryExpression(kind, Expression(node.Operand));
		}

		public static Sharp.PrefixUnaryExpressionSyntax PrefixUnary(PrefixUnaryExpression node)
		{
			CSyntaxKind kind = GetPrefixUnaryKind(node.Kind);
			return SyntaxFactory.PrefixUnaryExpression(kind, Expression(node.Operand));
		}

		public static Sharp.BinaryExpressionSyntax Binary(BinaryExpressionSyntax node)
		{
			CSyntaxKind kind = GetBinaryKind(node.Kind);
			return SyntaxFactory.BinaryExpression(kind, Expression(node.Left), Expression(node.Right));
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
						SyntaxFactory.Literal(node.Value.Text, (string)node.Value.Value!)),

				SyntaxKind.CharLiteralExpression
					=> SyntaxFactory.LiteralExpression(CSyntaxKind.CharacterLiteralExpression,
						SyntaxFactory.Literal(node.Value.Text, (char)node.Value.Value!)),

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
				SyntaxKind.CharKeyword => SyntaxFactory.PredefinedType(Token(CSyntaxKind.CharKeyword)),
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
				GenericNameSyntax g => SyntaxFactory.GenericName(
					SyntaxFactory.Identifier(g.Identifier.Text),
					SyntaxFactory.TypeArgumentList(
							SyntaxFactory.SeparatedList(g.TypeArgumentList.Arguments.Select(Type)))),

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

		private static CSyntaxKind GetAssignmentKind(SyntaxKind kind)
		{
			return kind switch
			{
				SyntaxKind.AssignmentExpression => CSyntaxKind.SimpleAssignmentExpression,
				SyntaxKind.AddAssignmentExpression => CSyntaxKind.AddAssignmentExpression,
				SyntaxKind.SubtractAssignmentExpression => CSyntaxKind.SubtractAssignmentExpression,
				SyntaxKind.MultiplyAssignmentExpression => CSyntaxKind.MultiplyAssignmentExpression,
				SyntaxKind.DivideAssignmentExpression => CSyntaxKind.DivideAssignmentExpression,
				SyntaxKind.ModuloAssignmentExpression => CSyntaxKind.ModuloAssignmentExpression,
				SyntaxKind.ExclusiveOrAssignmentExpression => CSyntaxKind.ExclusiveOrAssignmentExpression,
				SyntaxKind.AndAssignmentExpression => CSyntaxKind.AndAssignmentExpression,
				SyntaxKind.OrAssignmentExpression => CSyntaxKind.OrAssignmentExpression,
				SyntaxKind.LeftShiftAssignmentExpression => CSyntaxKind.LeftShiftAssignmentExpression,
				SyntaxKind.RightShiftAssignmentExpression => CSyntaxKind.RightShiftAssignmentExpression,
				SyntaxKind.UnsignedRightShiftAssignmentExpression => CSyntaxKind.UnsignedRightShiftAssignmentExpression,
				_ => throw new UnreachableException()
			};
		}

		private static CSyntaxKind GetPrefixUnaryKind(SyntaxKind kind)
		{
			return kind switch
			{
				SyntaxKind.UnaryPlusExpression => CSyntaxKind.UnaryPlusExpression,
				SyntaxKind.UnaryMinusExpression => CSyntaxKind.UnaryMinusExpression,
				SyntaxKind.PreIncrementExpression => CSyntaxKind.PreIncrementExpression,
				SyntaxKind.PreDecrementExpression => CSyntaxKind.PreDecrementExpression,
				SyntaxKind.LogicalNotExpression => CSyntaxKind.LogicalNotExpression,
				SyntaxKind.BitwiseNotExpression => CSyntaxKind.BitwiseNotExpression,
				_ => throw new UnreachableException()
			};
		}

		private static CSyntaxKind GetPostfixUnaryKind(SyntaxKind kind)
		{
			return kind switch
			{
				SyntaxKind.PostIncrementExpression => CSyntaxKind.PostIncrementExpression,
				SyntaxKind.PostDecrementExpression => CSyntaxKind.PostDecrementExpression,
				_ => throw new UnreachableException()
			};
		}

		private static CSyntaxKind GetBinaryKind(SyntaxKind kind)
		{
			return kind switch
			{
				SyntaxKind.AddExpression => CSyntaxKind.AddExpression,
				SyntaxKind.SubtractExpression => CSyntaxKind.SubtractExpression,
				SyntaxKind.MultiplyExpression => CSyntaxKind.MultiplyExpression,
				SyntaxKind.DivideExpression => CSyntaxKind.DivideExpression,
				SyntaxKind.ModuloExpression => CSyntaxKind.ModuloExpression,
				SyntaxKind.ExclusiveOrExpression => CSyntaxKind.ExclusiveOrExpression,
				SyntaxKind.BitwiseOrExpression => CSyntaxKind.BitwiseOrExpression,
				SyntaxKind.BitwiseAndExpression => CSyntaxKind.BitwiseAndExpression,
				SyntaxKind.RightShiftExpression => CSyntaxKind.RightShiftExpression,
				SyntaxKind.LeftShiftExpression => CSyntaxKind.LeftShiftExpression,
				SyntaxKind.UnsignedRightShiftExpression => CSyntaxKind.UnsignedRightShiftExpression,
				SyntaxKind.EqualsExpression => CSyntaxKind.EqualsExpression,
				SyntaxKind.NotEqualsExpression => CSyntaxKind.NotEqualsExpression,
				SyntaxKind.LessThanExpression => CSyntaxKind.LessThanExpression,
				SyntaxKind.LessThanOrEqualExpression => CSyntaxKind.LessThanOrEqualExpression,
				SyntaxKind.GreaterThanExpression => CSyntaxKind.GreaterThanExpression,
				SyntaxKind.GreaterThanOrEqualExpression => CSyntaxKind.GreaterThanOrEqualExpression,
				SyntaxKind.LogicalOrExpression => CSyntaxKind.LogicalOrExpression,
				SyntaxKind.LogicalAndExpression => CSyntaxKind.LogicalAndExpression,
				_ => throw new UnreachableException()
			};
		}
	}
}
