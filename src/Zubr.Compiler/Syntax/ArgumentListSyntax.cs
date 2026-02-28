using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ArgumentListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ArgumentList;

	public Token OpenParenToken { get; }

	public SeparatedSyntaxList<ArgumentSyntax> Arguments { get; }

	public Token CloseParenToken { get; }

	internal ArgumentListSyntax(SyntaxTree tree, TextSpan span, Token openParenToken, SeparatedSyntaxList<ArgumentSyntax> arguments, Token closeParenToken) : base(tree, span)
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
