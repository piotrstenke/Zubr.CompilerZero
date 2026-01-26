using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ExpressionStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;

	public ExpressionSyntax Expression { get; }

	public SyntaxToken SemicolonToken { get; }

	internal ExpressionStatementSyntax(ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		Expression = expression;
		SemicolonToken = semicolonToken;

		SetParent(expression);
	}

	public override string ToString()
	{
		return $"{Expression}{SemicolonToken}";
	}
}
