using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ExpressionStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;

	public ExpressionSyntax Expression { get; }

	public Token SemicolonToken { get; }

	internal ExpressionStatementSyntax(SyntaxTree tree, TextSpan span, ExpressionSyntax expression, Token semicolonToken) : base(tree, span)
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
