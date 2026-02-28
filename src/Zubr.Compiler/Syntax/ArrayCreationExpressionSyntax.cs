using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ArrayCreationExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ArrayCreationExpression;

	public Token NewKeyword { get; }

	public TypeSyntax? ElementType { get; }

	public SyntaxList<ArrayRankSyntax> Ranks { get; }

	public InitializerExpressionSyntax? Initializer { get; }

	internal ArrayCreationExpressionSyntax(
		SyntaxTree tree,
		TextSpan span,
		Token newKeyword,
		TypeSyntax? elementType,
		SyntaxList<ArrayRankSyntax> ranks,
		InitializerExpressionSyntax? initializer
	) : base(tree, span)
	{
		NewKeyword = newKeyword;
		ElementType = elementType;
		Ranks = ranks;
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
