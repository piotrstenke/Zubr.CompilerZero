using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class LetTypeSyntax : TypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.LetType;

	public Token LetKeyword { get; }

	internal LetTypeSyntax(SyntaxTree tree, TextSpan span, Token letKeyword) : base(tree, span)
	{
		LetKeyword = letKeyword;
	}

	public override string ToString()
	{
		return $"{LetKeyword}";
	}
}
