namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class BaseEnumDeclarationSyntax : BaseTypeDeclarationSyntax
{
	public abstract SeparatedSyntaxList<EnumMemberDeclarationSyntax> Members { get; }

	internal BaseEnumDeclarationSyntax()
	{
	}
}
