using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class CastExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.CastExpression;

	public Token OpenParenToken { get; }

	public TypeSyntax Type { get; }

	public Token CloseParenToken { get; }

	public ExpressionSyntax Expression { get; }

	internal CastExpressionSyntax(Token openParenToken, TypeSyntax type, Token closeParenToken, ExpressionSyntax expression)
	{
		OpenParenToken = openParenToken;
		Type = type;
		CloseParenToken = closeParenToken;
		Expression = expression;

		SetParent(type);
		SetParent(expression);
	}

	public override string ToString()
	{
		return $"{OpenParenToken}{Type}{CloseParenToken}{Expression}";
	}
}
