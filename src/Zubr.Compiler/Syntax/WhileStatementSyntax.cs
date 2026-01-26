using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class WhileStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.WhileStatement;

	public SyntaxToken WhileKeyword { get; }

	public SyntaxToken OpenParenToken { get; }

	public ExpressionSyntax Condition { get; }

	public SyntaxToken CloseParenToken { get; }

	public StatementSyntax Statement { get; }

	internal WhileStatementSyntax(SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		WhileKeyword = whileKeyword;
		OpenParenToken = openParenToken;
		Condition = condition;
		CloseParenToken = closeParenToken;
		Statement = statement;

		SetParent(condition);
		SetParent(statement);
	}

	public override string ToString()
	{
		return $"{WhileKeyword}{OpenParenToken}{Condition}{CloseParenToken} {Statement}";
	}
}
