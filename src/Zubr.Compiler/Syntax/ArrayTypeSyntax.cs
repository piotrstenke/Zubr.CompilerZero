using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ArrayTypeSyntax : TypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ArrayType;

	public TypeSyntax ElementType { get; }

	public SyntaxList<ArrayRankSyntax> Ranks { get; }

	internal ArrayTypeSyntax(TypeSyntax elementType, SyntaxList<ArrayRankSyntax> ranks)
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
