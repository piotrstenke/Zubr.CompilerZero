using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class InvocationExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.InvocationExpression;

	public ExpressionSyntax Expression { get; }

	public ArgumentListSyntax ArgumentList { get; }

	internal InvocationExpressionSyntax(ExpressionSyntax expression, ArgumentListSyntax argumentList)
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
