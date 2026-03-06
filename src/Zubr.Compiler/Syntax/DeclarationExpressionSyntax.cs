using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class DeclarationExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.DeclarationExpression;

	public TypeSyntax Type { get; }

	public Token Identifier { get; }

	internal DeclarationExpressionSyntax(SyntaxTree tree, TextSpan span, TypeSyntax type, Token identifier) : base(tree, span)
	{
		Type = type;
		Identifier = identifier;

		SetParent(type);
	}

	public override string ToString()
	{
		return $"{Type} {Identifier}";
	}
}
