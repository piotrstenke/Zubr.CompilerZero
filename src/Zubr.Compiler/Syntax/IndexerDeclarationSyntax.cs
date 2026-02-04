using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class IndexerDeclarationSyntax : BasePropertyDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.PropertyDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public override TypeSyntax Type { get; }

	public Token SelfKeyword { get; }

	public BracketParameterListSyntax ParameterList { get; }

	public override ArrowExpressionClauseSyntax? ExpressionBody { get; }

	public override AccessorListSyntax? AccessorList { get; }

	public override Token SemicolonToken { get; }

	internal IndexerDeclarationSyntax(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		TypeSyntax type,
		Token selfKeyword,
		BracketParameterListSyntax parameterList,
		ArrowExpressionClauseSyntax? expressionBody,
		AccessorListSyntax? accessorList,
		Token semicolonToken
	)
	{
		Attributes = attributes;
		Modifiers = modifiers;
		Type = type;
		SelfKeyword = selfKeyword;
		ParameterList = parameterList;
		ExpressionBody = expressionBody;
		AccessorList = accessorList;
		SemicolonToken = semicolonToken;

		SetParent(attributes);
		SetParent(type);
		SetParent(parameterList);
		SetParentIfNotNull(expressionBody);
		SetParentIfNotNull(accessorList);
	}

	public override string ToString()
	{
		if (ExpressionBody is not null)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Type} {SelfKeyword} {ParameterList} {ExpressionBody}{SemicolonToken}";
		}

		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Type} {SelfKeyword} {ParameterList}{(AccessorList is null ? "" : $" {AccessorList}")}{SemicolonToken}";
	}
}
