using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class BaseExpressionSyntax : InstanceExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.BaseExpression;

	public Token Keyword { get; }

	internal BaseExpressionSyntax(Token keyword)
	{
		Keyword = keyword;
	}

	public override string ToString()
	{
		return $"{Keyword}";
	}
}
