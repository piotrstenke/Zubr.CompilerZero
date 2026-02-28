using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class InitializerExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.InitializerExpression;

	public Token OpenBraceToken { get; }

	public SeparatedSyntaxList<ExpressionSyntax> Expressions { get; }

	public Token CloseBraceToken { get; }

	internal InitializerExpressionSyntax(
		SyntaxTree tree,
		TextSpan span,
		Token openBraceToken,
		SeparatedSyntaxList<ExpressionSyntax> expressions,
		Token closeBraceToken
	) : base(tree, span)
	{
		OpenBraceToken = openBraceToken;
		Expressions = expressions;
		CloseBraceToken = closeBraceToken;

		SetParent(expressions);
	}

	public override string ToString()
	{
		return $"{OpenBraceToken} ... {CloseBraceToken}";
	}
}
