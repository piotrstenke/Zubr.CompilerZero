using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class TypeParameterInlineConstraintSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeParameterInlineConstraint;

	public Token ColonToken { get; }

	public TypeParameterConstraintSyntax Constraint { get; }

	internal TypeParameterInlineConstraintSyntax(SyntaxTree tree, TextSpan span, Token colonToken, TypeParameterConstraintSyntax constraint) : base(tree, span)
	{
		ColonToken = colonToken;
		Constraint = constraint;

		SetParent(constraint);
	}

	public override string ToString()
	{
		return $"{ColonToken} {Constraint}";
	}
}
