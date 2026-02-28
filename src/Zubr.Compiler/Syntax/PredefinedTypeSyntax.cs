using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class PredefinedTypeSyntax : TypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.PredefinedType;

	public Token Keyword { get; }

	internal PredefinedTypeSyntax(SyntaxTree tree, TextSpan span, Token keyword) : base(tree, span)
	{
		Keyword = keyword;
	}

	public override string ToString()
	{
		return $"{Keyword}";
	}
}
