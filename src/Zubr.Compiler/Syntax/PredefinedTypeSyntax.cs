using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class PredefinedTypeSyntax : TypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.PredefinedType;

	public Token Keyword { get; }

	internal PredefinedTypeSyntax(Token keyword)
	{
		Keyword = keyword;
	}

	public override string ToString()
	{
		return $"{Keyword}";
	}
}
