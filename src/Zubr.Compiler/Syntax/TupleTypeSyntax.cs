using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class TupleTypeSyntax : TypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.TypeArgumentList;

	public Token OpenParenToken { get; }

	public SeparatedSyntaxList<TupleElementSyntax> Elements { get; }

	public Token CloseParenToken { get; }

	internal TupleTypeSyntax(SyntaxTree tree, TextSpan span, Token openParenToken, SeparatedSyntaxList<TupleElementSyntax> elements, Token closeParenToken) : base(tree, span)
	{
		OpenParenToken = openParenToken;
		Elements = elements;
		CloseParenToken = closeParenToken;

		SetParent(elements);
	}

	public override string ToString()
	{
		return $"{OpenParenToken}{Elements}{CloseParenToken}";
	}
}
