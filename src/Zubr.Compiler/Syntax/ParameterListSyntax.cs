using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ParameterListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ParameterList;

	public Token OpenParenToken { get; }

	public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }

	public Token CloseParenToken { get; }

	internal ParameterListSyntax(Token openParenToken, SeparatedSyntaxList<ParameterSyntax> parameters, Token closeParenToken)
	{
		OpenParenToken = openParenToken;
		Parameters = parameters;
		CloseParenToken = closeParenToken;

		SetParent(parameters);
	}

	public override string ToString()
	{
		return $"{OpenParenToken}{Parameters}{CloseParenToken}";
	}
}
