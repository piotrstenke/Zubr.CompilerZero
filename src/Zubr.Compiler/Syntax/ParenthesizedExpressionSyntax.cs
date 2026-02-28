using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;

	public Token OpenParenToken { get; }

	public ExpressionSyntax Expression { get; }

	public Token CloseParenToken { get; }

	internal ParenthesizedExpressionSyntax(SyntaxTree tree, TextSpan span, Token openParenToken, ExpressionSyntax expression, Token closeParenToken) : base(tree, span)
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
