namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class ComplexEnumDeclarationSyntax : BaseEnumDeclarationSyntax
{
	public abstract ParameterListSyntax? ParameterList { get; }

	internal ComplexEnumDeclarationSyntax()
	{
	}
}
