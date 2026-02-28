using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class TypeSyntax : ExpressionSyntax
{
	internal TypeSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{
	}
}
