using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class QualifiedNameSyntax : NameSyntax
{
	public override SyntaxKind Kind => SyntaxKind.QualifiedName;

	public NameSyntax Left { get; }

	public Token DotToken { get; }

	public SimpleNameSyntax Right { get; }

	internal QualifiedNameSyntax(NameSyntax left, Token dotToken, SimpleNameSyntax right)
	{
		Left = left;
		DotToken = dotToken;
		Right = right;

		SetParent(left);
		SetParent(right);
	}

	public override string ToString()
	{
		return $"{Left}{DotToken}{Right}";
	}
}
