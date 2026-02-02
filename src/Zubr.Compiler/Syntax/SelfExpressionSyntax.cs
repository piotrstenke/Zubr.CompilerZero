using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class SelfExpressionSyntax : InstanceExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.SelfExpression;

	public Token Keyword { get; }

	internal SelfExpressionSyntax(Token keyword)
	{
		Keyword = keyword;
	}

	public override string ToString()
	{
		return $"{Keyword}";
	}
}
