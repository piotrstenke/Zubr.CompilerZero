using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class WhileStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.WhileStatement;

	public Token WhileKeyword { get; }

	public Token OpenParenToken { get; }

	public ExpressionSyntax Condition { get; }

	public Token CloseParenToken { get; }

	public StatementSyntax Statement { get; }

	internal WhileStatementSyntax(SyntaxTree tree, TextSpan span, Token whileKeyword, Token openParenToken, ExpressionSyntax condition, Token closeParenToken, StatementSyntax statement) : base(tree, span)
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
