using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class PrefixUnaryExpression : ExpressionSyntax
{
	public override SyntaxKind Kind { get; }

	public SyntaxToken OperatorToken { get; }

	public ExpressionSyntax Operand { get; }

	internal PrefixUnaryExpression(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax operand)
	{
		Kind = kind;
		OperatorToken = operatorToken;
		Operand = operand;

		SetParent(operand);
	}

	public override string ToString()
	{
		return $"{OperatorToken}{Operand}";
	}
}
