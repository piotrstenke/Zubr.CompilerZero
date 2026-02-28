using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ElementAccessExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ElementAccessExpression;

	public ExpressionSyntax Expression { get; }

	public BracketArgumentListSyntax ArgumentList { get; }

	internal ElementAccessExpressionSyntax(SyntaxTree tree, TextSpan span, ExpressionSyntax expression, BracketArgumentListSyntax argumentList) : base(tree, span)
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
