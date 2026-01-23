using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ParameterListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ParameterList;

	public SyntaxToken OpenParenToken { get; }

	public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }

	public SyntaxToken CloseParenToken { get; }

	internal ParameterListSyntax(SyntaxToken openParenToken, SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closeParenToken)
	{
		OpenParenToken = openParenToken;
		Parameters = parameters;
		CloseParenToken = closeParenToken;

		SetParent(parameters);
	}
}
