using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class SimpleEnumMemberDeclarationSyntax : EnumMemberDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.SimpleEnumMemberDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public override Token Identifier { get; }

	public EqualsValueClauseSyntax? Initializer { get; }

	internal SimpleEnumMemberDeclarationSyntax(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		Token identifier,
		EqualsValueClauseSyntax? initializer
	)
	{
		Attributes = attributes;
		Modifiers = modifiers;
		Identifier = identifier;
		Initializer = initializer;

		SetParent(attributes);
		SetParentIfNotNull(initializer);
	}

	public override string ToString()
	{
		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Identifier}{(Initializer is null ? "" : $" {Initializer}")}";
	}
}
