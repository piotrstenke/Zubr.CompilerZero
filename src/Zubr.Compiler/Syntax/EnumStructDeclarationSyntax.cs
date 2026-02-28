using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class EnumStructDeclarationSyntax : ComplexEnumDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.EnumStructDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public override Token Keyword { get; }

	public Token StructKeyword { get; }

	public override Token Identifier { get; }

	public override ParameterListSyntax? ParameterList { get; }

	public override BaseTypeListSyntax? BaseTypeList { get; }

	public override Token SemicolonToken { get; }

	public override Token OpenBraceToken { get; }

	public override SeparatedSyntaxList<EnumMemberDeclarationSyntax> Members { get; }

	public override Token CloseBraceToken { get; }

	internal EnumStructDeclarationSyntax(
		SyntaxTree tree,
		TextSpan span,
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		Token keyword,
		Token structKeyword,
		Token identifier,
		ParameterListSyntax? parameterList,
		BaseTypeListSyntax? baseTypeList,
		Token semicolonToken,
		Token openBraceToken,
		SeparatedSyntaxList<EnumMemberDeclarationSyntax> members,
		Token closeBraceToken
	) : base(tree, span)
	{
		Attributes = attributes;
		Modifiers = modifiers;
		Keyword = keyword;
		StructKeyword = structKeyword;
		Identifier = identifier;
		ParameterList = parameterList;
		BaseTypeList = baseTypeList;
		SemicolonToken = semicolonToken;
		OpenBraceToken = openBraceToken;
		Members = members;
		CloseBraceToken = closeBraceToken;

		SetParent(attributes);
		SetParentIfNotNull(parameterList);
		SetParentIfNotNull(baseTypeList);
		SetParent(members);
	}

	public override string ToString()
	{
		if (SemicolonToken.IsFound)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword} {StructKeyword} {Identifier}{ParameterList}{(BaseTypeList is null ? "" : $" {BaseTypeList}")}{SemicolonToken}";
		}

		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword} {StructKeyword} {Identifier}{ParameterList}{(BaseTypeList is null ? "" : $" {BaseTypeList}")} {OpenBraceToken} ... {CloseBraceToken}";
	}
}
