namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class MemberDeclarationSyntax : NameSyntax
{
	public abstract SyntaxList<AttributeSyntax> Attributes { get; }

	public abstract TokenList Modifiers { get; }

	internal MemberDeclarationSyntax()
	{
	}
}
