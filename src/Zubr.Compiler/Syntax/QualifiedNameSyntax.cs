using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class QualifiedNameSyntax : NameSyntax
{
	public override SyntaxKind Kind => SyntaxKind.QualifiedName;

	public NameSyntax Left { get; }

	public Token DotToken { get; }

	public SimpleNameSyntax Right { get; }

	internal QualifiedNameSyntax(SyntaxTree tree, TextSpan span, NameSyntax left, Token dotToken, SimpleNameSyntax right) : base(tree, span)
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
