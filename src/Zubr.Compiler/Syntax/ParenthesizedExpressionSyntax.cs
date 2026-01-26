using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;

	public SyntaxToken OpenParenToken { get; }

	public ExpressionSyntax Expression { get; }

	public SyntaxToken CloseParenToken { get; }

	internal ParenthesizedExpressionSyntax(SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
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
