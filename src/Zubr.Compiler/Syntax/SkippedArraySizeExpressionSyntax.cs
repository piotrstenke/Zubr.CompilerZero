using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class SkippedArraySizeExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.SkippedArraySizeExpression;

	public Token Token { get; }

	internal SkippedArraySizeExpressionSyntax(SyntaxTree tree, TextSpan span, Token token) : base(tree, span)
	{
		Token = token;
	}

	public override string ToString()
	{
		return string.Empty;
	}
}
