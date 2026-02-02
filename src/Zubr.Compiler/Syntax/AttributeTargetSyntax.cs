namespace Zubr.Compiler.Syntax;

public sealed class AttributeTargetSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.AttributeTarget;

	public Token Keyword { get; }

	public Token ColonToken { get; }

	internal AttributeTargetSyntax(
		Token keyword,
		Token colonToken
	)
	{
		Keyword = keyword;
		ColonToken = colonToken;
	}

	public override string ToString()
	{
		return $"{Keyword}{ColonToken}";
	}
}
