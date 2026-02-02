using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class AttributeDeclarationSyntax : TypeDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.AttributeDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public override Token Keyword { get; }

	public override Token Identifier { get; }

	public override TypeParameterListSyntax? TypeParameterList { get; }

	public override ParameterListSyntax? ParameterList { get; }

	public override BaseTypeListSyntax? BaseTypeList { get; }

	public override TypeParameterConstraintListSyntax? ConstraintList { get; }

	public override Token SemicolonToken { get; }

	public override Token OpenBraceToken { get; }

	public override SyntaxList<MemberDeclarationSyntax> Members { get; }

	public override Token CloseBraceToken { get; }

	internal AttributeDeclarationSyntax(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		Token keyword,
		Token identifier,
		TypeParameterListSyntax? typeParameterList,
		ParameterListSyntax? parameterList,
		BaseTypeListSyntax? baseTypeList,
		TypeParameterConstraintListSyntax? constraintList,
		Token semicolonToken,
		Token openBraceToken,
		SyntaxList<MemberDeclarationSyntax> members,
		Token closeBraceToken
	)
	{
		Attributes = attributes;
		Modifiers = modifiers;
		Keyword = keyword;
		Identifier = identifier;
		TypeParameterList = typeParameterList;
		ParameterList = parameterList;
		BaseTypeList = baseTypeList;
		ConstraintList = constraintList;
		SemicolonToken = semicolonToken;
		OpenBraceToken = openBraceToken;
		Members = members;
		CloseBraceToken = closeBraceToken;

		SetParent(attributes);
		SetParentIfNotNull(typeParameterList);
		SetParentIfNotNull(parameterList);
		SetParentIfNotNull(baseTypeList);
		SetParentIfNotNull(constraintList);
		SetParent(members);
	}

	public override string ToString()
	{
		if (SemicolonToken.IsFound)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword} {Identifier}{TypeParameterList}{ParameterList}{(BaseTypeList is null ? "" : $" {BaseTypeList}")}{(ConstraintList is null ? "" : $" {ConstraintList}")}{SemicolonToken}";
		}

		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword} {Identifier}{TypeParameterList}{ParameterList}{(BaseTypeList is null ? "" : $" {BaseTypeList}")}{(ConstraintList is null ? "" : $" {ConstraintList}")} {OpenBraceToken} ... {CloseBraceToken}";
	}
}
