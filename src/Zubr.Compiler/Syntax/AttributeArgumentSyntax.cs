using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class AttributeArgumentSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.Attribute;

	public ExpressionSyntax Expression { get; }

	internal AttributeArgumentSyntax(
		ExpressionSyntax expression
	)
	{
		Expression = expression;

		SetParent(expression);
	}

	public override string ToString()
	{
		return $"{Expression}";
	}
}
