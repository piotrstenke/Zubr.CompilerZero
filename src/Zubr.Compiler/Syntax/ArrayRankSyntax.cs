using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ArrayRankSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ArrayRank;

	public Token OpenBracketToken { get; }
	
	public SeparatedSyntaxList<ExpressionSyntax> Sizes { get; }

	public Token CloseBracketToken { get; }

	internal ArrayRankSyntax(Token openBracketToken, SeparatedSyntaxList<ExpressionSyntax> sizes, Token closeBracketToken)
	{
		OpenBracketToken = openBracketToken;
		Sizes = sizes;
		CloseBracketToken = closeBracketToken;

		SetParent(sizes);
	}

	public override string ToString()
	{
		return $"{OpenBracketToken}{Sizes}{CloseBracketToken}";
	}
}
