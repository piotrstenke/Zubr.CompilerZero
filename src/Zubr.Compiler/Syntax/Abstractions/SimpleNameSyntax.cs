namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class SimpleNameSyntax : NameSyntax
{
	public abstract Token Identifier { get; }
}
