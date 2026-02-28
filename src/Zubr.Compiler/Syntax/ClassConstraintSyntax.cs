using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ClassConstraintSyntax : TypeParameterConstraintSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ClassConstraint;

	public Token ClassKeyword { get; }

	internal ClassConstraintSyntax(SyntaxTree tree, TextSpan span, Token classKeyword) : base(tree, span)
	{
		ClassKeyword = classKeyword;
	}

	public override string ToString()
	{
		return $"{ClassKeyword}";
	}
}
