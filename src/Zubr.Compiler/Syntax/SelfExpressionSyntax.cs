using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class SelfExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.SelfExpression;

	public SyntaxToken SelfKeyword { get; }

	internal SelfExpressionSyntax(SyntaxToken selfKeyword)
	{
		SelfKeyword = selfKeyword;
	}

	public override string ToString()
	{
		return $"{SelfKeyword}";
	}
}
