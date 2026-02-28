using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ArrowExpressionClauseSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ArrowExpressionClause;

	public Token ArrowToken { get; }

	public ExpressionSyntax Expression { get; }

	internal ArrowExpressionClauseSyntax(SyntaxTree tree, TextSpan span, Token arrowToken, ExpressionSyntax expression) : base(tree, span)
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
