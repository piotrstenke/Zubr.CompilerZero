using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class BaseExpressionSyntax : InstanceExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.BaseExpression;

	public Token Keyword { get; }

	internal BaseExpressionSyntax(SyntaxTree tree, TextSpan span, Token keyword) : base(tree, span)
	{
		Keyword = keyword;
	}

	public override string ToString()
	{
		return $"{Keyword}";
	}
}
