using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ParameterSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.Parameter;

	public SyntaxTokenList Modifiers { get; }

	public TypeSyntax Type { get; }

	public SyntaxToken Identifier { get; }

	public EqualsValueClauseSyntax? Default { get; }

	internal ParameterSyntax(SyntaxTokenList modifiers, TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax? @default)
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
