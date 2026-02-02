namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class BaseFunctionDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract ParameterListSyntax ParameterList { get; }

	public abstract BlockSyntax? Body { get; }

	public abstract ArrowExpressionClauseSyntax? ExpressionBody { get; }

	public abstract Token SemicolonToken { get; }

	internal BaseFunctionDeclarationSyntax()
	{
	}
}
