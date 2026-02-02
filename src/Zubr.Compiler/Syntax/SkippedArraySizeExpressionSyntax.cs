using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class SkippedArraySizeExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.SkippedArraySizeExpression;

	public Token Token { get; }

	internal SkippedArraySizeExpressionSyntax(Token token)
	{
		Token = token;
	}

	public override string ToString()
	{
		return string.Empty;
	}
}
