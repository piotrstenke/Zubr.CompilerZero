namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class SimpleNameSyntax : NameSyntax
{
	public abstract SyntaxToken Identifier { get; }
}
