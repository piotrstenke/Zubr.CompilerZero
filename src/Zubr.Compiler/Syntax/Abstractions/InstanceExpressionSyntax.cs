using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class InstanceExpressionSyntax : ExpressionSyntax
{
	internal InstanceExpressionSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{ 
	}
}
