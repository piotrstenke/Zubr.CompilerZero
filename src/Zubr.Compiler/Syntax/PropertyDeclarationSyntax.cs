using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class PropertyDeclarationSyntax : BasePropertyDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.PropertyDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public override TypeSyntax Type { get; }

	public Token Identifier { get; }

	public override ArrowExpressionClauseSyntax? ExpressionBody { get; }

	public override AccessorListSyntax? AccessorList { get; }

	public EqualsValueClauseSyntax? Initializer { get; }

	public override Token SemicolonToken { get; }

	internal PropertyDeclarationSyntax(
		SyntaxTree tree,
		TextSpan span,
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		TypeSyntax type,
		Token identifier,
		ArrowExpressionClauseSyntax? expressionBody,
		AccessorListSyntax? accessorList,
		EqualsValueClauseSyntax? initializer,
		Token semicolonToken
	) : base(tree, span)
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
		SetParentIfNotNull(expressionBody);
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
