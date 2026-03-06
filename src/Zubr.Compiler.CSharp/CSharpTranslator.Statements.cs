using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Zubr.Compiler.Syntax;
using Zubr.Compiler.Syntax.Abstractions;

using CSyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

using Sharp = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Zubr.Compiler.CSharp;

internal sealed partial class CSharpTranslator
{
	private static class Statements
	{
		public static Sharp.StatementSyntax Statement(StatementSyntax node, TypeContext context)
		{
			return node switch
			{
				BlockSyntax b => Block(b, context),
				ReturnStatementSyntax r => Return(r),
				StopStatementSyntax => Break(),
				NextStatementSyntax => Continue(),
				EmptyStatementSyntax => Empty(),
				IfStatementSyntax i => If(i, context),
				LocalDeclarationStatementSyntax l => Local(l),
				WhileStatementSyntax w => While(w, context),
				DoStatementSyntax d => Do(d, context),
				ForStatementSyntax f => For(f, context),
				RangedForStatementSyntax f => ForEach(f, context),
				ExpressionStatementSyntax expr => Expression(expr),
				FunctionDeclarationSyntax lf => LocalFunction(lf, context),
				GotoStatementSyntax g => Goto(g),
				LabelStatementSyntax l => Label(l, context),
				UnsafeStatementSyntax u => Unsafe(u, context),
				_ => throw new UnreachableException()
			};
		}

		public static Sharp.BlockSyntax Block(BlockSyntax node, TypeContext context)
		{
			return SyntaxFactory.Block(node.Statements
				.Select(x => StatementOrType(node, context)!)
				.Where(x => x is not null));
		}

		public static Sharp.ExpressionStatementSyntax Expression(ExpressionStatementSyntax node)
		{
			return SyntaxFactory.ExpressionStatement(Expressions.Expression(node.Expression));
		}

		public static Sharp.IfStatementSyntax If(IfStatementSyntax node, TypeContext context)
		{
			var elifs = node.Elifs;

			Sharp.ElseClauseSyntax? @else = node.Else is null
				? null
				: SyntaxFactory.ElseClause(Statement(node.Else.Statement, context));

			if (elifs.IsDefaultOrEmpty)
			{
				return SyntaxFactory.IfStatement(
					default,
					Expressions.Expression(node.Condition),
					Statement(node.Statement, context),
					@else
				);
			}

			ElifClauseSyntax last = elifs[^1];

			Sharp.IfStatementSyntax @if = SyntaxFactory.IfStatement(
				default,
				Expressions.Expression(last.Condition),
				Statement(last.Statement, context),
				@else
			);

			// The last one is already created, so start from second to last.
			for (int i = elifs.Count - 2; i >= 0; i--)
			{
				@if = SyntaxFactory.IfStatement(
					default,
					Expressions.Expression(elifs[i].Condition),
					Statement(elifs[i].Statement, context),
					SyntaxFactory.ElseClause(@if)
				);
			}

			return @if;
		}

		public static Sharp.GotoStatementSyntax Goto(GotoStatementSyntax node)
		{
			return SyntaxFactory.GotoStatement(CSyntaxKind.GotoStatement, SyntaxFactory.IdentifierName(node.Identifier.Text));
		}

		public static Sharp.LabeledStatementSyntax Label(LabelStatementSyntax node, TypeContext context)
		{
			Sharp.StatementSyntax? statement = StatementOrType(node, context);

			statement ??= SyntaxFactory.EmptyStatement();

			return SyntaxFactory.LabeledStatement(node.Identifier.Text, statement);
		}

		public static Sharp.WhileStatementSyntax While(WhileStatementSyntax node, TypeContext context)
		{
			return SyntaxFactory.WhileStatement(
				Expressions.Expression(node.Condition),
				Statement(node.Statement, context)
			);
		}

		public static Sharp.DoStatementSyntax Do(DoStatementSyntax node, TypeContext context)
		{
			return SyntaxFactory.DoStatement(
				Statement(node.Statement, context),
				Expressions.Expression(node.Condition)
			);
		}

		public static Sharp.CommonForEachStatementSyntax ForEach(RangedForStatementSyntax node, TypeContext context)
		{
			if (node.Variables[0] is VariableExpressionSyntax expr)
			{
				if(node.Variables.Count == 1)
				{
					Sharp.TypeSyntax elementType = GetElementType(expr);

					return SyntaxFactory.ForEachStatement(
						elementType,
						SyntaxFactory.Identifier(expr.Identifier.Text),
						Expressions.Expression(node.Expression),
						Statement(node.Statement, context)
					);
				}
				else
				{
					Sharp.ExpressionSyntax elementType = SyntaxFactory.TupleExpression(SyntaxFactory.SeparatedList(node.Variables
						.Cast<VariableExpressionSyntax>()
						.Select(x => SyntaxFactory.Argument(SyntaxFactory.DeclarationExpression(
							GetElementType(x),
							SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(x.Identifier.Text)))))));

					return SyntaxFactory.ForEachVariableStatement(
						elementType,
						SyntaxFactory.InvocationExpression(
							Expressions.GlobalMemberAccess("System", "Linq", "Enumerable", "Index"),
							SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
								SyntaxFactory.Argument(Expressions.Expression(node.Expression))))),
						Statement(node.Statement, context)
					);
				}
			}
			else
			{
				return SyntaxFactory.ForEachVariableStatement(
					Expressions.Expression(node.Variables[0]),
					Expressions.Expression(node.Expression),
					Statement(node.Statement, context)
				);
			}

			static Sharp.TypeSyntax GetElementType(VariableExpressionSyntax expr)
			{
				return expr.Type is null
					? SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("var"))
					: Expressions.Type(expr.Type);
			}
		}

		public static Sharp.ForStatementSyntax For(ForStatementSyntax node, TypeContext context)
		{
			return SyntaxFactory.ForStatement(
				attributeLists: default,
				node.Declaration is null ? null : Declarations.Variable(node.Declaration),
				node.Initializers.IsDefaultOrEmpty ? default : SyntaxFactory.SeparatedList(node.Initializers.Select(Expressions.Expression)),
				node.Condition is null ? null : Expressions.Expression(node.Condition),
				node.Incrementors.IsDefaultOrEmpty ? default : SyntaxFactory.SeparatedList(node.Incrementors.Select(Expressions.Expression)),
				Statement(node.Statement, context)
			);
		}

		public static Sharp.LocalDeclarationStatementSyntax Local(LocalDeclarationStatementSyntax node)
		{
			return SyntaxFactory.LocalDeclarationStatement(Declarations.Variable(node.Variable));
		}

		public static Sharp.LocalFunctionStatementSyntax LocalFunction(FunctionDeclarationSyntax node, TypeContext context)
		{
			Sharp.TypeParameterListSyntax? typeParameterList = Declarations.TypeParameterList(node.TypeParameterList);
			Sharp.ParameterListSyntax parameters = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(node.ParameterList.Parameters.Select(Declarations.Parameter)));

			var constraints = Declarations.ConstraintList(node.TypeParameterList, node.ConstraintList);

			return SyntaxFactory.LocalFunctionStatement(
				Declarations.Attributes(node.Attributes),
				Declarations.GetModifiers(node, node.Modifiers, out _),
				Expressions.Type(node.ReturnType),
				SyntaxFactory.Identifier(node.Identifier.Text),
				typeParameterList,
				parameters,
				constraints,
				node.Body is null ? null : Block(node.Body, context),
				node.ExpressionBody is null ? null : ExpressionBody(node.ExpressionBody),
				node.Body is null && node.ExpressionBody is null ? SyntaxFactory.Token(CSyntaxKind.SemicolonToken) : default
			);
		}

		public static Sharp.BreakStatementSyntax Break()
		{
			return SyntaxFactory.BreakStatement();
		}

		public static Sharp.EmptyStatementSyntax Empty()
		{
			return SyntaxFactory.EmptyStatement();
		}

		public static Sharp.ContinueStatementSyntax Continue()
		{
			return SyntaxFactory.ContinueStatement();
		}

		public static Sharp.UnsafeStatementSyntax Unsafe(UnsafeStatementSyntax node, TypeContext context)
		{
			return SyntaxFactory.UnsafeStatement(Block(node.Block, context));
		}

		public static Sharp.LockStatementSyntax Lock(LockStatementSyntax node, TypeContext context)
		{
			return SyntaxFactory.LockStatement(Expressions.Expression(node.Expression), Statement(node, context));
		}

		public static Sharp.ReturnStatementSyntax Return(ReturnStatementSyntax node)
		{
			if (node.Expression is null)
			{
				return SyntaxFactory.ReturnStatement();
			}

			return SyntaxFactory.ReturnStatement(Expressions.Expression(node.Expression));
		}

		public static Sharp.ArrowExpressionClauseSyntax ExpressionBody(ArrowExpressionClauseSyntax node)
		{
			return SyntaxFactory.ArrowExpressionClause(Expressions.Expression(node.Expression));
		}

		private static Sharp.StatementSyntax? StatementOrType(StatementSyntax node, TypeContext context)
		{
			if (node is BaseTypeDeclarationSyntax type)
			{
				context.AddMember(Declarations.Type(type));
				return null;
			}

			return Statement(node, context);
		}
	}
}
