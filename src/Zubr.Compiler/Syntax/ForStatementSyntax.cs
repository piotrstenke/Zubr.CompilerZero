using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ForStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ForStatement;

	public SyntaxToken ForKeyword { get; }

	public SyntaxToken OpenParenToken { get; }

	public ExpressionSyntax Variable { get; }

	public SyntaxToken ColonToken { get; }

	public ExpressionSyntax Expression { get; }

	public SyntaxToken CloseParenToken { get; }

	public StatementSyntax Statement { get; }


	internal ForStatementSyntax(SyntaxToken forKeyword, SyntaxToken openParenToken, ExpressionSyntax variable, SyntaxToken colonToken, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
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
}
