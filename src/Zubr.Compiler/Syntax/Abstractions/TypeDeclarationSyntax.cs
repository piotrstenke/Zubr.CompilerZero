namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class TypeDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract Token Keyword { get; }

	public abstract Token Identifier { get; }

	public abstract TypeParameterListSyntax? TypeParameterList { get; }

	public abstract TypeParameterConstraintListSyntax? ConstraintList { get; }

	public abstract Token OpenBraceToken { get; }

	public abstract Token CloseBraceToken { get; }

	public abstract SyntaxList<MemberDeclarationSyntax> Members { get; }
}
