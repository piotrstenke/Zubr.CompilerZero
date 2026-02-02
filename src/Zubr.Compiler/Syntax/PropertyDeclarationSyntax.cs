using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class PropertyDeclarationSyntax : MemberDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.PropertyDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public TypeSyntax Type { get; }

	public Token Identifier { get; }

	public ArrowExpressionClauseSyntax? ExpressionBody { get; }

	public AccessorListSyntax? AccessorList { get; }

	public EqualsValueClauseSyntax? Initializer { get; }

	public Token SemicolonToken { get; }

	internal PropertyDeclarationSyntax(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		TypeSyntax type,
		Token identifier,
		ArrowExpressionClauseSyntax? expressionBody,
		AccessorListSyntax? accessorList,
		EqualsValueClauseSyntax? initializer,
		Token semicolonToken
	)
	{
		Attributes = attributes;
		Modifiers = modifiers;
		Type = type;
		Identifier = identifier;
		ExpressionBody = expressionBody;
		AccessorList = accessorList;
		Initializer = initializer;
		SemicolonToken = semicolonToken;

		SetParent(attributes);
		SetParent(type);
		SetParentIfNotNull(accessorList);
		SetParentIfNotNull(accessorList);
		SetParentIfNotNull(initializer);
	}

	public override string ToString()
	{
		if(ExpressionBody is not null)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Type} {Identifier} {ExpressionBody}{SemicolonToken}";
		}

		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Type} {Identifier}{(AccessorList is null ? "" : $" {AccessorList}")}{(Initializer is null ? "" : $" {Initializer}")}{SemicolonToken}";
	}
}
