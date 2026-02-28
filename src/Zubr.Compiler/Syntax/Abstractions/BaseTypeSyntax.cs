using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class BaseTypeSyntax : SyntaxNode
{
	public abstract TypeSyntax Type { get; }
	
	internal BaseTypeSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{
	}
}
