using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class PrefixUnaryExpression : ExpressionSyntax
{
	public override SyntaxKind Kind { get; }

	public Token OperatorToken { get; }

	public ExpressionSyntax Operand { get; }

	internal PrefixUnaryExpression(SyntaxKind kind, Token operatorToken, ExpressionSyntax operand)
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
