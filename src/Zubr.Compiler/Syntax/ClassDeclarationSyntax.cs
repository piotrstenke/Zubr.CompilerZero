using System;
using System.Linq.Expressions;
using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ClassDeclarationSyntax : TypeDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ClassDeclaration;

	public override SyntaxTokenList Modifiers { get; }

	public override SyntaxToken Keyword { get; }

	public override SyntaxToken Identifier { get; }

	public override TypeParameterListSyntax? TypeParameterList { get; }

	public override TypeParameterConstraintListSyntax? ConstraintList { get; }

	public override SyntaxToken OpenBraceToken { get; }

	public override SyntaxToken CloseBraceToken { get; }

	public override SyntaxList<MemberDeclarationSyntax> Members { get; }

	internal ClassDeclarationSyntax(SyntaxTokenList modifiers, SyntaxToken keyword, SyntaxToken identifier, TypeParameterListSyntax? typeParameterList, TypeParameterConstraintListSyntax? constraintList, SyntaxToken openBraceToken, SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken)
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
