using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class TypeParameterConstraintClauseSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeParameterConstraintClause;

	public Token Identifier { get; }

	public Token ColonToken { get; }

	public SeparatedSyntaxList<TypeParameterConstraintSyntax> Constraints { get; }

	internal TypeParameterConstraintClauseSyntax(Token identifier, Token colonToken, SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints)
	{
		Identifier = identifier;
		ColonToken = colonToken;
		Constraints = constraints;

		SetParent(constraints);
	}

	public override string ToString()
	{
		return $"{Identifier} {ColonToken} {Constraints}";
	}
}
