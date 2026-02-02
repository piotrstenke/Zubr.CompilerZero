namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class BaseTypeSyntax : SyntaxNode
{
	public abstract TypeSyntax Type { get; }
	
	internal BaseTypeSyntax()
	{
	}
}
