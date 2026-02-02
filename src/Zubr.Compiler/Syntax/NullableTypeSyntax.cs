using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class NullableTypeSyntax : TypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.NullableType;

	public TypeSyntax ElementType { get; }

	public Token QuestionToken { get; }

	internal NullableTypeSyntax(TypeSyntax type, Token questionToken)
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
