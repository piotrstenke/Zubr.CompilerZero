using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ClassDeclarationSyntax : TypeDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ClassDeclaration;

	public override TokenList Modifiers { get; }

	public override Token Keyword { get; }

	public override Token Identifier { get; }

	public override TypeParameterListSyntax? TypeParameterList { get; }

	public override TypeParameterConstraintListSyntax? ConstraintList { get; }

	public override Token OpenBraceToken { get; }

	public override Token CloseBraceToken { get; }

	public override SyntaxList<MemberDeclarationSyntax> Members { get; }

	internal ClassDeclarationSyntax(TokenList modifiers, Token keyword, Token identifier, TypeParameterListSyntax? typeParameterList, TypeParameterConstraintListSyntax? constraintList, Token openBraceToken, SyntaxList<MemberDeclarationSyntax> members, Token closeBraceToken)
	{
		Modifiers = modifiers;
		Keyword = keyword;
		Identifier = identifier;
		TypeParameterList = typeParameterList;
		ConstraintList = constraintList;
		OpenBraceToken = openBraceToken;
		Members = members;
		CloseBraceToken = closeBraceToken;

		SetParent(members);
		SetParentIfNotNull(typeParameterList);
		SetParentIfNotNull(constraintList);
	}

	public override string ToString()
	{
		return $"{Modifiers} {Keyword} {Identifier}{TypeParameterList}{(ConstraintList is null ? "" : $" {ConstraintList}")} {OpenBraceToken} ... {CloseBraceToken}";
	}
}
