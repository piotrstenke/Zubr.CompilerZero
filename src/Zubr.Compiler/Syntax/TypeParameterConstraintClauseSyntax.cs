using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class TypeParameterConstraintClauseSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeParameterConstraintClause;

	public SyntaxToken Identifier { get; }

	public SyntaxToken ColonToken { get; }

	public SeparatedSyntaxList<TypeParameterConstraintSyntax> Constraints { get; }

	internal TypeParameterConstraintClauseSyntax(SyntaxToken identifier, SyntaxToken colonToken, SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints)
	{
		Identifier = identifier;
		ColonToken = colonToken;
		Constraints = constraints;

		SetParent(constraints);
	}
}
