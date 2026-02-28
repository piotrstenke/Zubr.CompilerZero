using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ArrayRankSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ArrayRank;

	public Token OpenBracketToken { get; }
	
	public SeparatedSyntaxList<ExpressionSyntax> Sizes { get; }

	public Token CloseBracketToken { get; }

	internal ArrayRankSyntax(SyntaxTree tree, TextSpan span, Token openBracketToken, SeparatedSyntaxList<ExpressionSyntax> sizes, Token closeBracketToken) : base(tree, span)
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
