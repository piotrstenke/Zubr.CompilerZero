using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class AttributeArgumentListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.AttributeArgumentList;

	public Token OpenParenToken { get; }

	public SeparatedSyntaxList<AttributeArgumentSyntax> Arguments { get; }

	public Token CloseParenToken { get; }

	internal AttributeArgumentListSyntax(
		Token openParenToken,
		SeparatedSyntaxList<AttributeArgumentSyntax> arguments,
		Token closeParenToken
	)
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
