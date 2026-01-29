using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ParameterSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.Parameter;

	public TokenList Modifiers { get; }

	public TypeSyntax Type { get; }

	public Token Identifier { get; }

	public EqualsValueClauseSyntax? Default { get; }

	internal ParameterSyntax(TokenList modifiers, TypeSyntax type, Token identifier, EqualsValueClauseSyntax? @default)
	{
		Modifiers = modifiers;
		Identifier = identifier;
		Type = type;
		Default = @default;

		SetParent(type);
		SetParentIfNotNull(@default);
	}

	public override string ToString()
	{
		return $"{Modifiers} {Type} {Identifier}{(Default is null ? "" : $" {Default}")}";
	}
}
