using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ClassDeclarationSyntax : TypeDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ClassDeclaration;

	public override SyntaxTokenList Modifiers { get; }

	public override SyntaxToken Keyword { get; }

	public override SyntaxToken Identifier { get; }

	public override SyntaxToken OpenBraceToken { get; }

	public override SyntaxToken CloseBraceToken { get; }

	public override SyntaxList<MemberDeclarationSyntax> Members { get; }

	internal ClassDeclarationSyntax(SyntaxTokenList modifiers, SyntaxToken keyword, SyntaxToken identifier, SyntaxToken openBraceToken, SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken)
	{
		Modifiers = modifiers;
		Keyword = keyword;
		Identifier = identifier;
		OpenBraceToken = openBraceToken;
		Members = members;
		CloseBraceToken = closeBraceToken;

		SetParent(members);
	}
}
