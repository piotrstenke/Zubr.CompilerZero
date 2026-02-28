using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class SelfExpressionSyntax : InstanceExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.SelfExpression;

	public Token Keyword { get; }

	internal SelfExpressionSyntax(SyntaxTree tree, TextSpan span, Token keyword) : base(tree, span)
	{
		Keyword = keyword;
	}

	public override string ToString()
	{
		return $"{Keyword}";
	}
}
