using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class SimpleEnumDeclarationSyntax : BaseEnumDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.EnumDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public override Token Keyword { get; }

	public override Token Identifier { get; }

	public override BaseTypeListSyntax? BaseTypeList { get; }

	public override Token SemicolonToken { get; }

	public override Token OpenBraceToken { get; }

	public override SeparatedSyntaxList<EnumMemberDeclarationSyntax> Members { get; }

	public override Token CloseBraceToken { get; }

	internal SimpleEnumDeclarationSyntax(
		SyntaxTree tree,
		TextSpan span,
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		Token keyword,
		Token identifier,
		Token semicolonToken,
		Token openBraceToken,
		SeparatedSyntaxList<EnumMemberDeclarationSyntax> members,
		Token closeBraceToken
	) : base(tree, span)
	{
		Modifiers = modifiers;
		Keyword = keyword;
		Identifier = identifier;
		SemicolonToken = semicolonToken;
		OpenBraceToken = openBraceToken;
		Members = members;
		CloseBraceToken = closeBraceToken;

		SetParent(attributes);
		SetParent(members);
	}

	public override string ToString()
	{
		if(SemicolonToken.IsAny)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword} {Identifier}{(BaseTypeList is null ? "" : $" {BaseTypeList}")}{SemicolonToken}";
		}

		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword} {Identifier}{(BaseTypeList is null ? "" : $" {BaseTypeList}")} {OpenBraceToken} ... {CloseBraceToken}";
	}
}
