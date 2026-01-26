using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class StructDeclarationSyntax : TypeDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.StructDeclaration;

	public override SyntaxTokenList Modifiers { get; }

	public override SyntaxToken Keyword { get; }

	public override SyntaxToken Identifier { get; }

	public override TypeParameterListSyntax? TypeParameterList { get; }

	public override TypeParameterConstraintListSyntax? ConstraintList { get; }

	public override SyntaxToken OpenBraceToken { get; }

	public override SyntaxToken CloseBraceToken { get; }

	public override SyntaxList<MemberDeclarationSyntax> Members { get; }

	internal StructDeclarationSyntax(SyntaxTokenList modifiers, SyntaxToken keyword, SyntaxToken identifier, TypeParameterListSyntax? typeParameterList, TypeParameterConstraintListSyntax? constraintList, SyntaxToken openBraceToken, SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken)
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
