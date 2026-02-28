using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class NameSyntax : TypeSyntax
{
	internal NameSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{
	}
}
