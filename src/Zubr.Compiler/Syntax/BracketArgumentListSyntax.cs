using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class BracketArgumentListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.BracketArgumentList;

	public Token OpenBracketToken { get; }

	public SeparatedSyntaxList<ArgumentSyntax> Arguments { get; }

	public Token CloseBracketToken { get; }

	internal BracketArgumentListSyntax(SyntaxTree tree, TextSpan span, Token openBracketToken, SeparatedSyntaxList<ArgumentSyntax> arguments, Token closeBracketToken) : base(tree, span)
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
