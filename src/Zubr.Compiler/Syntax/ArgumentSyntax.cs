using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ArgumentSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.Argument;

	public ExpressionSyntax Expression { get; }

	internal ArgumentSyntax(ExpressionSyntax expression)
	{
		Expression = expression;

		SetParent(expression);
	}

	public override string ToString()
	{
		return $"{Expression}";
	}
}
