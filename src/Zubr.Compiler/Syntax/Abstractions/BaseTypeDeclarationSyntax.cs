namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class BaseTypeDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract Token Keyword { get; }

	public abstract Token Identifier { get; }

	public abstract BaseTypeListSyntax? BaseTypeList { get; }

	public abstract Token OpenBraceToken { get; }

	public abstract Token CloseBraceToken { get; }

	public abstract Token SemicolonToken { get; }

	internal BaseTypeDeclarationSyntax()
	{
	}
}
