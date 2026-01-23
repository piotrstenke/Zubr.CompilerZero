using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind { get; }

	public SyntaxToken Value { get; }

	internal LiteralExpressionSyntax(SyntaxKind kind, SyntaxToken value)
	{
		Kind = kind;
		Value = value;
	}
}
