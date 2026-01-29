using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ArgumentListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ArgumentList;

	public Token OpenParenToken { get; }

	public SeparatedSyntaxList<ArgumentSyntax> Arguments { get; }

	public Token CloseParenToken { get; }

	internal ArgumentListSyntax(Token openParenToken, SeparatedSyntaxList<ArgumentSyntax> arguments, Token closeParenToken)
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
