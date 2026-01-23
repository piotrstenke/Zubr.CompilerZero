namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class TypeDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract SyntaxToken Keyword { get; }

	public abstract SyntaxToken Identifier { get; }

	public abstract TypeParameterListSyntax? TypeParameterList { get; }

	public abstract TypeParameterConstraintListSyntax? ConstraintList { get; }

	public abstract SyntaxToken OpenBraceToken { get; }

	public abstract SyntaxToken CloseBraceToken { get; }

	public abstract SyntaxList<MemberDeclarationSyntax> Members { get; }
}
