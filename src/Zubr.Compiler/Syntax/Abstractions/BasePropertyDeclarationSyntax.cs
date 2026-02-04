namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class BasePropertyDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract TypeSyntax Type { get; }

	public abstract ArrowExpressionClauseSyntax? ExpressionBody { get; }

	public abstract AccessorListSyntax? AccessorList { get; }

	public abstract Token SemicolonToken { get; }

	internal BasePropertyDeclarationSyntax()
	{
	}
}
