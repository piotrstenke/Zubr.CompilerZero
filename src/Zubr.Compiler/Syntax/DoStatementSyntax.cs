using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class DoStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.DoStatement;

	public SyntaxToken DoKeyword { get; }

	public StatementSyntax Statement { get; }

	public SyntaxToken WhileKeyword { get; }

	public SyntaxToken OpenParenToken { get; }

	public ExpressionSyntax Condition { get; }

	public SyntaxToken CloseParenToken { get; }

	public SyntaxToken SemicolonToken { get; }

	internal DoStatementSyntax(SyntaxToken doKeyword, StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken)
	{
		DoKeyword = doKeyword;
		Statement = statement;
		WhileKeyword= whileKeyword;
		OpenParenToken = openParenToken;
		Condition = condition;
		CloseParenToken = closeParenToken;
		SemicolonToken = semicolonToken;

		SetParent(statement);
		SetParent(condition);
	}
}
