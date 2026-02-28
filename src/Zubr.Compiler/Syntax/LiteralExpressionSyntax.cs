using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind { get; }

	public Token Value { get; }

	internal LiteralExpressionSyntax(SyntaxTree tree, TextSpan span, SyntaxKind kind, Token value) : base(tree, span)
	{
		Kind = kind;
		Value = value;
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
