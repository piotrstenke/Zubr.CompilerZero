using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ComplexEnumMemberDeclarationSyntax : EnumMemberDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ComplexEnumMemberDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public override Token Identifier { get; }

	public ArgumentListSyntax? ArgumentList { get; }

	internal ComplexEnumMemberDeclarationSyntax(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		Token identifier,
		ArgumentListSyntax? argumentList
	)
	{
		Attributes = attributes;
		Modifiers = modifiers;
		Identifier = identifier;
		ArgumentList = argumentList;

		SetParent(attributes);
		SetParentIfNotNull(argumentList);
	}

	public override string ToString()
	{
		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Identifier}{ArgumentList}";
	}
}
