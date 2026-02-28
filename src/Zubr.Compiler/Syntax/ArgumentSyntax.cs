using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ArgumentSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.Argument;

	public ExpressionSyntax Expression { get; }

	internal ArgumentSyntax(SyntaxTree tree, TextSpan span, ExpressionSyntax expression) : base(tree, span)
	{
		Expression = expression;

		SetParent(expression);
	}

	public override string ToString()
	{
		return $"{Expression}";
	}
}
