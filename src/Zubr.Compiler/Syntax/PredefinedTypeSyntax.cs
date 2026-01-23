using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class PredefinedTypeSyntax : TypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.PredefinedType;

	public SyntaxToken Keyword { get; }

	internal PredefinedTypeSyntax(SyntaxToken keyword)
	{
		Keyword = keyword;
	}
}
