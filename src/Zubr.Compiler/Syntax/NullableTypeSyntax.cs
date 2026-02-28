using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class NullableTypeSyntax : TypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.NullableType;

	public TypeSyntax ElementType { get; }

	public Token QuestionToken { get; }

	internal NullableTypeSyntax(SyntaxTree tree, TextSpan span, TypeSyntax type, Token questionToken) : base(tree, span)
	{
		ElementType = type;
		QuestionToken = questionToken;

		SetParent(type);
	}

	public override string ToString()
	{
		return $"{ElementType}{QuestionToken}";
	}
}
