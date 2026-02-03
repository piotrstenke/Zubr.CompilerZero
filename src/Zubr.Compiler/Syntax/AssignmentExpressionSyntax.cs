using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class AssignmentExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;

	public ExpressionSyntax Left { get; }

	public Token OperatorToken { get; }

	public ExpressionSyntax Right { get; }

	internal AssignmentExpressionSyntax(ExpressionSyntax left, Token operatorToken, ExpressionSyntax right)
	{
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
