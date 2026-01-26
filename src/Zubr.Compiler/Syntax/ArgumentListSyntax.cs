using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ArgumentListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ArgumentList;

	public SyntaxToken OpenParenToken { get; }

	public SeparatedSyntaxList<ArgumentSyntax> Arguments { get; }

	public SyntaxToken CloseParenToken { get; }

	internal ArgumentListSyntax(SyntaxToken openParenToken, SeparatedSyntaxList<ArgumentSyntax> arguments, SyntaxToken closeParenToken)
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
