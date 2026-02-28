using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class TypeConstraintSyntax : TypeParameterConstraintSyntax
{
	public override SyntaxKind Kind => SyntaxKind.TypeConstraint;

	public TypeSyntax Type { get; }

	internal TypeConstraintSyntax(SyntaxTree tree, TextSpan span, TypeSyntax type) : base(tree, span)
	{
		Type = type;

		SetParent(type);
	}

	public override string ToString()
	{
		return $"{Type}";
	}
}
