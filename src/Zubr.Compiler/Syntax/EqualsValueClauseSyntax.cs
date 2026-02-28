using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class EqualsValueClauseSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.EqualsValueClause;

	public Token EqualsToken { get; }

	public ExpressionSyntax Value { get; }

	internal EqualsValueClauseSyntax(SyntaxTree tree, TextSpan span, Token equalsToken, ExpressionSyntax value) : base(tree, span)
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
