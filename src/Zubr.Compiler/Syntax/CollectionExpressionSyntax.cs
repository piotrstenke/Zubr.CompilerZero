using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class CollectionExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.CollectionExpression;

	public Token OpenBracketToken { get; }

	public SeparatedSyntaxList<ExpressionSyntax> Elements { get; }

	public Token CloseBracketToken { get; }

	internal CollectionExpressionSyntax(
		Token openBracketToken,
		SeparatedSyntaxList<ExpressionSyntax> elements,
		Token closeBracketToken
	)
	{
		OpenBracketToken = openBracketToken;
		Elements = elements;
		CloseBracketToken = closeBracketToken;

		SetParent(elements);
	}

	public override string ToString()
	{
		return $"{OpenBracketToken}{Elements}{CloseBracketToken}";
	}
}
