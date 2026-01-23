using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class StructConstraintSyntax : TypeParameterConstraintSyntax
{
	public override SyntaxKind Kind => SyntaxKind.StructConstraint;

	public SyntaxToken StructKeyword { get; }

	internal StructConstraintSyntax(SyntaxToken structKeyword)
	{
		StructKeyword = structKeyword;
	}
}
