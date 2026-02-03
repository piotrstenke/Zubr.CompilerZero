using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ElementAccessExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ElementAccessExpression;

	public ExpressionSyntax Expression { get; }

	public BracketArgumentListSyntax ArgumentList { get; }

	internal ElementAccessExpressionSyntax(ExpressionSyntax expression, BracketArgumentListSyntax argumentList)
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
