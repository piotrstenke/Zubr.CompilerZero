using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class AttributeArgumentSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.Attribute;

	public ExpressionSyntax Expression { get; }

	internal AttributeArgumentSyntax(
		SyntaxTree tree,
		TextSpan span,
		ExpressionSyntax expression
	) : base(tree, span)
	{
		Expression = expression;

		SetParent(expression);
	}

	public override string ToString()
	{
		return $"{Expression}";
	}
}
