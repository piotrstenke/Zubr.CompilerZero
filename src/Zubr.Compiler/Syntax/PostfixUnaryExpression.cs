using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class PostfixUnaryExpression : ExpressionSyntax
{
	public override SyntaxKind Kind { get; }

	public ExpressionSyntax Operand { get; }

	public SyntaxToken OperatorToken { get; }

	internal PostfixUnaryExpression(SyntaxKind kind, ExpressionSyntax operand, SyntaxToken operatorToken)
	{
		Kind = kind;
		Operand = operand;
		OperatorToken = operatorToken;
	}

	public override string ToString()
	{
		return $"{Operand}{OperatorToken}";
	}
}
