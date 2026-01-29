using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;

	public Token OpenParenToken { get; }

	public ExpressionSyntax Expression { get; }

	public Token CloseParenToken { get; }

	internal ParenthesizedExpressionSyntax(Token openParenToken, ExpressionSyntax expression, Token closeParenToken)
	{
		OpenParenToken = openParenToken;
		Expression = expression;
		CloseParenToken = closeParenToken;

		SetParent(expression);
	}

	public override string ToString()
	{
		return $"{OpenParenToken}{Expression}{CloseParenToken}";
	}
}
