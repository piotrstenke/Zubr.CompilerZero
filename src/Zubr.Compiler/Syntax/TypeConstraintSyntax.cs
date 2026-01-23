using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class TypeConstraintSyntax : TypeParameterConstraintSyntax
{
	public override SyntaxKind Kind => SyntaxKind.TypeConstraint;

	public TypeSyntax Type { get; }

	internal TypeConstraintSyntax(TypeSyntax type)
	{
		Type = type;

		SetParent(type);
	}
}
