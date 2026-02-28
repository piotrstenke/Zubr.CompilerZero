using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ParameterListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ParameterList;

	public Token OpenParenToken { get; }

	public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }

	public Token CloseParenToken { get; }

	internal ParameterListSyntax(SyntaxTree tree, TextSpan span, Token openParenToken, SeparatedSyntaxList<ParameterSyntax> parameters, Token closeParenToken) : base(tree, span)
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
