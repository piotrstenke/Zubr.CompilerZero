using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Linq;
using Zubr.Compiler.Syntax;
using Zubr.Compiler.Syntax.Abstractions;

using Sharp = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Zubr.Compiler.CSharp;

internal sealed partial class CSharpTranslator
{
	private static class Statements
	{
		public static Sharp.StatementSyntax Statement(StatementSyntax node)
		{
			return node switch
			{
				BlockSyntax b => Block(b),
				ReturnStatementSyntax r => Return(r),
				StopStatementSyntax s => Break(s),
				NextStatementSyntax n => Continue(n),
				EmptyStatementSyntax e => Empty(e),
				IfStatementSyntax i => If(i),
				LocalDeclarationStatementSyntax l => Local(l),
				WhileStatementSyntax w => While(w),
				DoStatementSyntax d => Do(d),
				ForStatementSyntax f => ForEach(f),
				_ => throw new UnreachableException()
			};
		}

		public static Sharp.BlockSyntax Block(BlockSyntax node)
		{
			return SyntaxFactory.Block(node.Statements.Select(Statement));
		}

		public static Sharp.IfStatementSyntax If(IfStatementSyntax node)
		{
			var elifs = node.Elifs;

			Sharp.ElseClauseSyntax? @else = node.Else is null
				? null
				: SyntaxFactory.ElseClause(Statement(node.Else.Statement));

			if (elifs.IsDefaultOrEmpty)
			{
				return SyntaxFactory.IfStatement(
					default,
					Expressions.Expression(node.Condition),
					Statement(node.Statement),
					@else
				);
			}

			ElifClauseSyntax last = elifs[^1];

			Sharp.IfStatementSyntax @if = SyntaxFactory.IfStatement(
				default,
				Expressions.Expression(last.Condition),
				Statement(last.Statement),
				@else
			);

			// The last one is already created, so start from second to last.
			for (int i = elifs.Count - 2; i >= 0; i--)
			{
				@if = SyntaxFactory.IfStatement(
					default,
					Expressions.Expression(elifs[i].Condition),
					Statement(elifs[i].Statement),
					SyntaxFactory.ElseClause(@if)
				);
			}

			return @if;
		}

		public static Sharp.WhileStatementSyntax While(WhileStatementSyntax node)
		{
			return SyntaxFactory.WhileStatement(
				Expressions.Expression(node.Condition),
				Statement(node.Statement)
			);
		}

		public static Sharp.DoStatementSyntax Do(DoStatementSyntax node)
		{
			return SyntaxFactory.DoStatement(
				Statement(node.Statement),
				Expressions.Expression(node.Condition)
			);
		}

		public static Sharp.CommonForEachStatementSyntax ForEach(ForStatementSyntax node)
		{
			if(node.Variable is VariableExpressionSyntax expr)
			{
				Sharp.TypeSyntax type = expr.Type is null
					? SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("var"))
					: Expressions.Type(expr.Type);

				return SyntaxFactory.ForEachStatement(
					type,
					SyntaxFactory.Identifier(expr.Identifier.Text),
					Expressions.Expression(node.Expression),
					Statement(node.Statement)
				);
			}

			return SyntaxFactory.ForEachVariableStatement(
				Expressions.Expression(node.Variable),
				Expressions.Expression(node.Expression),
				Statement(node.Statement)
			);
		}

		public static Sharp.LocalDeclarationStatementSyntax Local(LocalDeclarationStatementSyntax node)
		{
			return SyntaxFactory.LocalDeclarationStatement(
				SyntaxFactory.VariableDeclaration(Expressions.Type(node.Declaration.Type),
				SyntaxFactory.SingletonSeparatedList(
					SyntaxFactory.VariableDeclarator(
						SyntaxFactory.Identifier(node.Declaration.Variable.Identifier.Text),
						null,
						SyntaxFactory.EqualsValueClause(Expressions.Expression(node.Declaration.Variable.Initializer.Value))
				)))
			);
		}

		public static Sharp.BreakStatementSyntax Break(StopStatementSyntax node)
		{
			return SyntaxFactory.BreakStatement();
		}

		public static Sharp.EmptyStatementSyntax Empty(EmptyStatementSyntax node)
		{
			return SyntaxFactory.EmptyStatement();
		}

		public static Sharp.ContinueStatementSyntax Continue(NextStatementSyntax node)
		{
			return SyntaxFactory.ContinueStatement();
		}

		public static Sharp.ReturnStatementSyntax Return(ReturnStatementSyntax node)
		{
			if (node.Expression is null)
			{
				return SyntaxFactory.ReturnStatement();
			}

			return SyntaxFactory.ReturnStatement(Expressions.Expression(node.Expression));
		}
	}
}
