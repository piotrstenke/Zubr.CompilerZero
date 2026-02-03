using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class BracketArgumentListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.BracketArgumentList;

	public Token OpenBracketToken { get; }

	public SeparatedSyntaxList<ArgumentSyntax> Arguments { get; }

	public Token CloseBracketToken { get; }

	internal BracketArgumentListSyntax(Token openBracketToken, SeparatedSyntaxList<ArgumentSyntax> arguments, Token closeBracketToken)
	{
		OpenBracketToken = openBracketToken;
		Arguments = arguments;
		CloseBracketToken = closeBracketToken;

		SetParent(arguments);
	}

	public override string ToString()
	{
		return $"{OpenBracketToken}{Arguments}{CloseBracketToken}";
	}
}
