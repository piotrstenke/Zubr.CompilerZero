using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ClassConstraintSyntax : TypeParameterConstraintSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ClassConstraint;

	public SyntaxToken ClassKeyword { get; }

	internal ClassConstraintSyntax(SyntaxToken classKeyword)
	{
		ClassKeyword = classKeyword;
	}

	public override string ToString()
	{
		return $"{ClassKeyword}";
	}
}
