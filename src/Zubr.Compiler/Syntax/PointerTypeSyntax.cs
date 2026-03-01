using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class PointerTypeSyntax : TypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.PointerType;

	public TypeSyntax ElementType { get; }

	public Token AsteriskToken { get; }

	internal PointerTypeSyntax(SyntaxTree tree, TextSpan span, TypeSyntax elementType, Token asteriskToken) : base(tree, span)
	{
		ElementType = elementType;
		AsteriskToken = asteriskToken;

		SetParent(elementType);
	}

	public override string ToString()
	{
		return $"{ElementType}{AsteriskToken}";
	}
}
