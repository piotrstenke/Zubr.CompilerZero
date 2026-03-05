using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class EqualsTypeClauseSyntax : SyntaxNode
{
	public override SyntaxKind Kind =>  SyntaxKind.EqualsTypeClause;

	public Token EqualsToken { get; }

	public TypeSyntax Type { get; }

	internal EqualsTypeClauseSyntax(SyntaxTree tree, TextSpan span, Token equalsToken, TypeSyntax type) : base(tree, span)
	{
		EqualsToken = equalsToken;
		Type = type;

		SetParent(type);
	}

	public override string ToString()
	{
		return $"{EqualsToken} {Type}";
	}
}
