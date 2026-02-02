using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class EqualsValueClauseSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.EqualsValueClause;

	public Token EqualsToken { get; }

	public ExpressionSyntax Value { get; }

	internal EqualsValueClauseSyntax(Token equalsToken, ExpressionSyntax value)
	{
		EqualsToken = equalsToken;
		Value = value;

		SetParent(value);
	}

	public override string ToString()
	{
		return $"{EqualsToken} {Value}";
	}
}
