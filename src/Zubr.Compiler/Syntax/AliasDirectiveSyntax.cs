using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class AliasDirectiveSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.AliasDirective;

	public Token AliasKeyword { get; }

	public NameSyntax Name { get; }

	public Token EqualsToken { get; }

	public TypeSyntax Type { get; }

	public Token SemicolonToken { get; }

	internal AliasDirectiveSyntax(Token aliasKeyword, NameSyntax name, Token equalsToken, TypeSyntax type, Token semicolonToken)
	{
		AliasKeyword = aliasKeyword;
		Name = name;
		EqualsToken = equalsToken;
		Type = type;
		SemicolonToken = semicolonToken;

		SetParent(name);
		SetParent(type);
	}

	public override string ToString()
	{
		return $"{AliasKeyword} {Name} {EqualsToken} {Type}{SemicolonToken}";
	}
}
