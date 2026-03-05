using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class TypeParameterSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeParameter;

	public SyntaxList<AttributeSyntax> Attributes { get; }

	public Token Identifier { get; }

	public TypeParameterInlineConstraintSyntax? InlineConstraint { get; }

	public EqualsTypeClauseSyntax? DefaultType { get; }

	internal TypeParameterSyntax(
		SyntaxTree tree,
		TextSpan span,
		SyntaxList<AttributeSyntax> attributes,
		Token identifier,
		TypeParameterInlineConstraintSyntax? inlineConstraint,
		EqualsTypeClauseSyntax? defaultType
	) : base(tree, span)
	{
		Attributes = attributes;
		Identifier = identifier;
		InlineConstraint = inlineConstraint;
		DefaultType = defaultType;

		SetParent(attributes);
		SetParentIfNotNull(inlineConstraint);
		SetParentIfNotNull(defaultType);
	}

	public override string ToString()
	{
		return $"{Identifier}{(InlineConstraint is null ? "" : $" {InlineConstraint}")}{(DefaultType is null ? "" : $" {DefaultType}")}";
	}
}
