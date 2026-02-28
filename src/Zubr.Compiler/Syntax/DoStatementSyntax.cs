using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class DoStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.DoStatement;

	public Token DoKeyword { get; }

	public StatementSyntax Statement { get; }

	public Token WhileKeyword { get; }

	public Token OpenParenToken { get; }

	public ExpressionSyntax Condition { get; }

	public Token CloseParenToken { get; }

	public Token SemicolonToken { get; }

	internal DoStatementSyntax(SyntaxTree tree, TextSpan span, Token doKeyword, StatementSyntax statement, Token whileKeyword, Token openParenToken, ExpressionSyntax condition, Token closeParenToken, Token semicolonToken) : base(tree, span)
	{
		DoKeyword = doKeyword;
		Statement = statement;
		WhileKeyword = whileKeyword;
		OpenParenToken = openParenToken;
		Condition = condition;
		CloseParenToken = closeParenToken;
		SemicolonToken = semicolonToken;

		SetParent(statement);
		SetParent(condition);
	}

	public override string ToString()
	{
		return $"{DoKeyword} {Statement} {WhileKeyword}{OpenParenToken}{Condition}{CloseParenToken}{SemicolonToken}";
	}
}
