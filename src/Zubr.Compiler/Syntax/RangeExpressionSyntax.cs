using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class RangeExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.RangeExpression;

	public ExpressionSyntax Left { get; }

	public Token RangeToken { get; }

	public Token ComparisonToken { get; }

	public ExpressionSyntax Right { get; }

	internal RangeExpressionSyntax(
		SyntaxTree tree,
		TextSpan span,
		ExpressionSyntax left,
		Token rangeToken,
		Token comparisonToken,
		ExpressionSyntax right
	) : base(tree, span)
	{
		Left = left;
		RangeToken = rangeToken;
		ComparisonToken = comparisonToken;
		Right = right;

		SetParent(left);
		SetParent(right);
	}

	public override string ToString()
	{
		return $"{Left}{RangeToken}{Right}";
	}
}
