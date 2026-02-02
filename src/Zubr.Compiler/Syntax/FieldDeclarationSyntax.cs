using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class FieldDeclarationSyntax : MemberDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.FieldDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public Token FieldKeyword { get; }

	public VariableDeclarationSyntax Variable { get; }

	public Token SemicolonToken { get; }

	internal FieldDeclarationSyntax(
		SyntaxList<AttributeSyntax> attributes,
		Token fieldKeyword,
		VariableDeclarationSyntax variable,
		Token semicolonToken
	)
	{
		Attributes = attributes;
		FieldKeyword = fieldKeyword;
		Variable = variable;
		SemicolonToken = semicolonToken;

		SetParent(attributes);
		SetParent(variable);
	}

	public override string ToString()
	{
		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{FieldKeyword} {Variable}{SemicolonToken}";
	}
}
