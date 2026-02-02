using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ArrayCreationExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ArrayCreationExpression;

	public Token NewKeyword { get; }

	public TypeSyntax? ElementType { get; }

	public SyntaxList<ArrayRankSyntax> Ranks { get; }

	public InitializerExpressionSyntax? Initializer { get; }

	internal ArrayCreationExpressionSyntax(
		Token newKeyword,
		TypeSyntax? elementType,
		SyntaxList<ArrayRankSyntax> ranks,
		InitializerExpressionSyntax? initializer
	)
	{
		NewKeyword = newKeyword;
		ElementType = elementType;
		Initializer = initializer;

		SetParentIfNotNull(elementType);
		SetParent(ranks);
		SetParentIfNotNull(initializer);
	}

	public override string ToString()
	{
		return $"{NewKeyword}{(ElementType is null ? "" : $" {ElementType}")}{Ranks}{(Initializer is null ? "" : $" {Initializer}")}";
	}
}
