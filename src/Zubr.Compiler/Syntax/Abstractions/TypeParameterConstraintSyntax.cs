using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class TypeParameterConstraintSyntax : SyntaxNode
{
	internal TypeParameterConstraintSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{
	}
}
