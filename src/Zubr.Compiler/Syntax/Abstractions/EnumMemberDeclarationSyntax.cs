namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class EnumMemberDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract Token Identifier { get; }

	internal EnumMemberDeclarationSyntax()
	{
	}
}
