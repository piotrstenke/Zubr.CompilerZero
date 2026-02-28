using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class AttributeArgumentListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.AttributeArgumentList;

	public Token OpenParenToken { get; }

	public SeparatedSyntaxList<AttributeArgumentSyntax> Arguments { get; }

	public Token CloseParenToken { get; }

	internal AttributeArgumentListSyntax(
		SyntaxTree tree,
		TextSpan span,
		Token openParenToken,
		SeparatedSyntaxList<AttributeArgumentSyntax> arguments,
		Token closeParenToken
	) : base(tree, span)
	{
		OpenParenToken = openParenToken;
		Arguments = arguments;
		CloseParenToken = closeParenToken;

		SetParent(arguments);
	}

	public override string ToString()
	{
		return $"{OpenParenToken}{Arguments}{CloseParenToken}";
	}
}
