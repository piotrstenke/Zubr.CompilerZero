using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind { get; }

	public ExpressionSyntax Left { get; }

	public SyntaxToken OperatorToken { get; }

	public ExpressionSyntax Right { get; }

	internal BinaryExpressionSyntax(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
	{
		Kind = kind;
		Left = left;
		OperatorToken = operatorToken;
		Right = right;

		SetParent(left);
		SetParent(right);
	}

	public override string ToString()
	{
		return $"{Left} {OperatorToken} {Right}";
	}
}
