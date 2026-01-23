namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class MemberDeclarationSyntax : NameSyntax
{
	public abstract SyntaxTokenList Modifiers { get; }
}
