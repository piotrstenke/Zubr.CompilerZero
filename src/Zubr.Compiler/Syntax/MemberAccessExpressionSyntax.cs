using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class MemberAccessExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.MemberAccessExpression;

	public ExpressionSyntax Expression { get; }

	public Token OperatorToken { get; }

	public SimpleNameSyntax Name { get; }

	internal MemberAccessExpressionSyntax(SyntaxTree tree, TextSpan span, ExpressionSyntax expression, Token operatorToken, SimpleNameSyntax name) : base(tree, span)
	{
		Expression = expression;
		OperatorToken = operatorToken;
		Name = name;

		SetParent(expression);
		SetParent(name);
	}

	public override string ToString()
	{
		return $"{Expression}{OperatorToken}{Name}";
	}
}
