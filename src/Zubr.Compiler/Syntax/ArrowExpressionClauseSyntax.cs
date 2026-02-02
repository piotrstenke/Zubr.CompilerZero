using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ArrowExpressionClauseSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ArrowExpressionClause;

	public Token ArrowToken { get; }

	public ExpressionSyntax Expression { get; }

	internal ArrowExpressionClauseSyntax(Token arrowToken, ExpressionSyntax expression)
	{
		ArrowToken = arrowToken;
		Expression = expression;

		SetParent(expression);
	}

	public override string ToString()
	{
		return $"{ArrowToken} {Expression}";
	}
}
