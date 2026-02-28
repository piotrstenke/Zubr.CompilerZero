using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class SimpleNameSyntax : NameSyntax
{
	public abstract Token Identifier { get; }

	internal SimpleNameSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{
	}
}
