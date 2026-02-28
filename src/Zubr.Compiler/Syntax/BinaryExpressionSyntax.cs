using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind { get; }

	public ExpressionSyntax Left { get; }

	public Token OperatorToken { get; }

	public ExpressionSyntax Right { get; }

	internal BinaryExpressionSyntax(SyntaxTree tree, TextSpan span, SyntaxKind kind, ExpressionSyntax left, Token operatorToken, ExpressionSyntax right) : base(tree, span)
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
