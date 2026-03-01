using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ReferenceTypeSyntax : TypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ReferenceType;

	public TypeSyntax ElementType { get; }

	public Token AmpersandToken { get; }

	internal ReferenceTypeSyntax(SyntaxTree tree, TextSpan span, TypeSyntax elementType, Token ampersandToken) : base(tree, span)
	{
		ElementType = elementType;
		AmpersandToken = ampersandToken;

		SetParent(elementType);
	}

	public override string ToString()
	{
		return $"{ElementType}{AmpersandToken}";
	}
}
