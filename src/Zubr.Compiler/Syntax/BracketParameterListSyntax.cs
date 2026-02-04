using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class BracketParameterListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.BracketParameterList;

	public Token OpenBracketToken { get; }

	public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }

	public Token CloseBracketToken { get; }

	internal BracketParameterListSyntax(Token openBracketToken, SeparatedSyntaxList<ParameterSyntax> parameters, Token closeBracketToken)
	{
		OpenBracketToken = openBracketToken;
		Parameters = parameters;
		CloseBracketToken = closeBracketToken;

		SetParent(parameters);
	}

	public override string ToString()
	{
		return $"{OpenBracketToken}{Parameters}{CloseBracketToken}";
	}
}
