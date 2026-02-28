using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class InvocationExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.InvocationExpression;

	public ExpressionSyntax Expression { get; }

	public ArgumentListSyntax ArgumentList { get; }

	internal InvocationExpressionSyntax(SyntaxTree tree, TextSpan span, ExpressionSyntax expression, ArgumentListSyntax argumentList) : base(tree, span)
	{
		Expression = expression;
		ArgumentList = argumentList;

		SetParent(expression);
		SetParent(argumentList);
	}

	public override string ToString()
	{
		return $"{Expression}{ArgumentList}";
	}
}
