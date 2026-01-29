using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ForStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ForStatement;

	public Token ForKeyword { get; }

	public Token OpenParenToken { get; }

	public ExpressionSyntax Variable { get; }

	public Token ColonToken { get; }

	public ExpressionSyntax Expression { get; }

	public Token CloseParenToken { get; }

	public StatementSyntax Statement { get; }

	internal ForStatementSyntax(Token forKeyword, Token openParenToken, ExpressionSyntax variable, Token colonToken, ExpressionSyntax expression, Token closeParenToken, StatementSyntax statement)
	{
		ForKeyword = forKeyword;
		OpenParenToken = openParenToken;
		Variable = variable;
		ColonToken = colonToken;
		Expression = expression;
		CloseParenToken = closeParenToken;
		Statement = statement;

		SetParent(variable);
		SetParent(expression);
		SetParent(statement);
	}

	public override string ToString()
	{
		return $"{ForKeyword}{OpenParenToken}{Variable} {ColonToken} {Expression}{CloseParenToken} {Statement}";
	}
}
