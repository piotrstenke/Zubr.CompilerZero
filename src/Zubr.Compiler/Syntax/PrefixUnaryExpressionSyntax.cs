using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class PrefixUnaryExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind { get; }

	public Token OperatorToken { get; }

	public ExpressionSyntax Operand { get; }

	internal PrefixUnaryExpressionSyntax(SyntaxTree tree, TextSpan span, SyntaxKind kind, Token operatorToken, ExpressionSyntax operand) : base(tree, span)
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
