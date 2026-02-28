using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class TypeParameterConstraintClauseSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeParameterConstraintClause;

	public Token Identifier { get; }

	public Token ColonToken { get; }

	public SeparatedSyntaxList<TypeParameterConstraintSyntax> Constraints { get; }

	internal TypeParameterConstraintClauseSyntax(SyntaxTree tree, TextSpan span, Token identifier, Token colonToken, SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints) : base(tree, span)
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
