using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class TypeParameterConstraintListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeParameterConstraintList;

	public Token WhereKeyword { get; }

	public SeparatedSyntaxList<TypeParameterConstraintClauseSyntax> Clauses { get; }

	internal TypeParameterConstraintListSyntax(Token whereKeyword, SeparatedSyntaxList<TypeParameterConstraintClauseSyntax> constraints)
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
