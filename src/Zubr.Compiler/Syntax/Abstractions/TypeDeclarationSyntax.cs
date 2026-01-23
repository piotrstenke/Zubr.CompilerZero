namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class TypeDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract SyntaxToken Keyword { get; }

	public abstract SyntaxToken Identifier { get; }

	public abstract SyntaxToken OpenBraceToken { get; }

	public abstract SyntaxToken CloseBraceToken { get; }

	public abstract SyntaxList<MemberDeclarationSyntax> Members { get; }
}
