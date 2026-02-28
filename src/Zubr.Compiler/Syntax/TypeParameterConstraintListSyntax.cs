using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class TypeParameterConstraintListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeParameterConstraintList;

	public Token WhereKeyword { get; }

	public SeparatedSyntaxList<TypeParameterConstraintClauseSyntax> Clauses { get; }

	internal TypeParameterConstraintListSyntax(SyntaxTree tree, TextSpan span, Token whereKeyword, SeparatedSyntaxList<TypeParameterConstraintClauseSyntax> constraints) : base(tree, span)
	{
		WhereKeyword = whereKeyword;
		Clauses = constraints;

		SetParent(constraints);
	}

	public override string ToString()
	{
		return $"{WhereKeyword} {Clauses}";
	}
}
