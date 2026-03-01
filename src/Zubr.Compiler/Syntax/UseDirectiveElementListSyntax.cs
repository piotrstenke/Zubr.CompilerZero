using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class UseDirectiveElementListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.UseDirectiveElementList;

	public Token OpenBraceToken { get; }

	public SeparatedSyntaxList<UseDirectiveElementSyntax> Elements { get; }

	public Token CloseBraceToken { get; }

	internal UseDirectiveElementListSyntax(
		SyntaxTree tree,
		TextSpan span,
		Token openBraceToken,
		SeparatedSyntaxList<UseDirectiveElementSyntax> elements,
		Token closeBraceToken
	) : base(tree, span)
	{
		OpenBraceToken = openBraceToken;
		Elements = elements;
		CloseBraceToken = closeBraceToken;

		SetParent(elements);
	}

	public override string ToString()
	{
		return $"{OpenBraceToken} {Elements} {CloseBraceToken}";
	}
}
