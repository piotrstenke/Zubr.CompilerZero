using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class LockStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.LockStatement;

	public Token LockKeyword { get; }

	public Token OpenParenToken { get; }

	public ExpressionSyntax Expression { get; }

	public Token CloseParenToken { get; }

	public StatementSyntax Statement { get; }

	internal LockStatementSyntax(
		SyntaxTree tree,
		TextSpan span,
		Token lockKeyword,
		Token openParenToken,
		ExpressionSyntax expression,
		Token closeParen,
		StatementSyntax statement
	) : base(tree, span)
	{
		LockKeyword = lockKeyword;
		OpenParenToken = openParenToken;
		Expression = expression;
		CloseParenToken = closeParen;
		Statement = statement;

		SetParent(expression);
		SetParent(statement);
	}
}
