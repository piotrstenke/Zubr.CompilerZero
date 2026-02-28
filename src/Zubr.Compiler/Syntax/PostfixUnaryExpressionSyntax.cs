using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class PostfixUnaryExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind { get; }

	public ExpressionSyntax Operand { get; }

	public Token OperatorToken { get; }

	internal PostfixUnaryExpressionSyntax(SyntaxTree tree, TextSpan span, SyntaxKind kind, ExpressionSyntax operand, Token operatorToken) : base(tree, span)
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
