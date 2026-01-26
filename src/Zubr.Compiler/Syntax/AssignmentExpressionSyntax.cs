using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class AssignmentExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;

	public ExpressionSyntax Left { get; }

	public SyntaxToken OperatorToken { get; }

	public ExpressionSyntax Right { get; }

	internal AssignmentExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
	{
		Left = left;
		OperatorToken = operatorToken;
		Right = right;

		SetParent(left);
		SetParent(right);
	}
}
