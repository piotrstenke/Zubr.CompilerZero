using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind { get; }

	public Token Value { get; }

	internal LiteralExpressionSyntax(SyntaxKind kind, Token value)
	{
		Kind = kind;
		Value = value;
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
