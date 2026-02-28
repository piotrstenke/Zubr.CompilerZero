using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class StatementSyntax : SyntaxNode
{
	internal StatementSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{
	}
}
