using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ReturnStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ReturnStatement;

	public Token ReturnKeyword { get; }

	public ExpressionSyntax? Expression { get; }

	public Token SemicolonToken { get; }

	internal ReturnStatementSyntax(Token returnKeyword, ExpressionSyntax? expression, Token semicolonToken)
	{
		ReturnKeyword = returnKeyword;
		Expression = expression;
		SemicolonToken = semicolonToken;

		SetParentIfNotNull(expression);
	}

	public override string ToString()
	{
		return $"{ReturnKeyword} {Expression}{SemicolonToken}";
	}
}
