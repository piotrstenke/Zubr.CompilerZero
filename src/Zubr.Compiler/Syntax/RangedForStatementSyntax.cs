using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class RangedForStatementSyntax : BaseForStatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.RangedForStatement;

	public override Token ForKeyword { get; }

	public override Token OpenParenToken { get; }

	public SeparatedSyntaxList<ExpressionSyntax> Variables { get; }

	public Token ColonToken { get; }

	public ExpressionSyntax Expression { get; }

	public override Token CloseParenToken { get; }

	public override StatementSyntax Statement { get; }

	internal RangedForStatementSyntax(
		SyntaxTree tree,
		TextSpan span,
		Token forKeyword,
		Token openParenToken,
		SeparatedSyntaxList<ExpressionSyntax> variables,
		Token colonToken,
		ExpressionSyntax expression,
		Token closeParenToken,
		StatementSyntax statement
	) : base(tree, span)
	{
		ForKeyword = forKeyword;
		OpenParenToken = openParenToken;
		Variables = variables;
		ColonToken = colonToken;
		Expression = expression;
		CloseParenToken = closeParenToken;
		Statement = statement;

		SetParent(variables);
		SetParent(expression);
		SetParent(statement);
	}

	public override string ToString()
	{
		return $"{ForKeyword}{OpenParenToken}{Variables} {ColonToken} {Expression}{CloseParenToken} {Statement}";
	}
}
