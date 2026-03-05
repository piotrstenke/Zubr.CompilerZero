using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ImplementationDeclarationSyntax : MemberDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ImplementationDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public Token Keyword { get; }

	public TypeParameterListSyntax? TypeParameterList { get; }

	public TypeSyntax Type { get; }

	public BaseTypeListSyntax? BaseTypeList { get; }

	public TypeParameterConstraintListSyntax? ConstraintList { get; }

	public Token SemicolonToken { get; }

	public Token OpenBraceToken { get; }

	public SyntaxList<MemberDeclarationSyntax> Members { get; }

	public Token CloseBraceToken { get; }

	internal ImplementationDeclarationSyntax(
		SyntaxTree tree,
		TextSpan span,
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		Token keyword,
		TypeParameterListSyntax? typeParameterList,
		TypeSyntax type,
		BaseTypeListSyntax? baseTypeList,
		TypeParameterConstraintListSyntax? constraintList,
		Token semicolonToken,
		Token openBraceToken,
		SyntaxList<MemberDeclarationSyntax> members,
		Token closeBraceToken
	) : base(tree, span)
	{
		Attributes = attributes;
		Modifiers = modifiers;
		Keyword = keyword;
		TypeParameterList = typeParameterList;
		Type = type;
		BaseTypeList = baseTypeList;
		ConstraintList = constraintList;
		SemicolonToken = semicolonToken;
		OpenBraceToken = openBraceToken;
		Members = members;
		CloseBraceToken = closeBraceToken;

		SetParent(attributes);
		SetParentIfNotNull(typeParameterList);
		SetParentIfNotNull(type);
		SetParentIfNotNull(baseTypeList);
		SetParentIfNotNull(constraintList);
		SetParent(members);
	}

	public override string ToString()
	{
		if (SemicolonToken.IsAny)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword} {Type}{(BaseTypeList is null ? "" : $" {BaseTypeList}")}{(ConstraintList is null ? "" : $" {ConstraintList}")}{SemicolonToken}";
		}

		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword} {Type}{(BaseTypeList is null ? "" : $" {BaseTypeList}")}{(ConstraintList is null ? "" : $" {ConstraintList}")} {OpenBraceToken} ... {CloseBraceToken}";
	}
}
