using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class SpreadExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.SpreadExpression;

	public Token Operator { get; }

	public ExpressionSyntax Expression { get; }

	internal SpreadExpressionSyntax(Token @operator, ExpressionSyntax expression)
	{
		Operator = @operator;
		Expression = expression;
	}

	public override string ToString()
	{
		return $"{Operator}{Expression}";
	}
}
