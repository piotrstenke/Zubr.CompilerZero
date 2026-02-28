using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class SpreadExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.SpreadExpression;

	public Token Operator { get; }

	public ExpressionSyntax Expression { get; }

	internal SpreadExpressionSyntax(SyntaxTree tree, TextSpan span, Token @operator, ExpressionSyntax expression) : base(tree, span)
	{
		Operator = @operator;
		Expression = expression;
	}

	public override string ToString()
	{
		return $"{Operator}{Expression}";
	}
}
