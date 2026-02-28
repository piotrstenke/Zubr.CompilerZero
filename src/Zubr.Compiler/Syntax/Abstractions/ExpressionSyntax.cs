using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class ExpressionSyntax : SyntaxNode
{
	internal ExpressionSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{
	}
}
