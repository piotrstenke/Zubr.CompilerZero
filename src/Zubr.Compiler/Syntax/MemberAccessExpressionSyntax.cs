using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class MemberAccessExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind { get; }

	public ExpressionSyntax Expression { get; }

	public Token OperatorToken { get; }

	public SimpleNameSyntax Name { get; }

	internal MemberAccessExpressionSyntax(SyntaxTree tree, TextSpan span, SyntaxKind kind, ExpressionSyntax expression, Token operatorToken, SimpleNameSyntax name) : base(tree, span)
	{
		Kind = kind;
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
