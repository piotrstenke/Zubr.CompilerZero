using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ArrayTypeSyntax : TypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ArrayType;

	public TypeSyntax ElementType { get; }

	public SyntaxList<ArrayRankSyntax> Ranks { get; }

	internal ArrayTypeSyntax(SyntaxTree tree, TextSpan span, TypeSyntax elementType, SyntaxList<ArrayRankSyntax> ranks) : base(tree, span)
	{
		ElementType = elementType;
		Ranks = ranks;

		SetParent(elementType);
		SetParent(ranks);
	}

	public override string ToString()
	{
		return $"{ElementType}{Ranks}";
	}
}
