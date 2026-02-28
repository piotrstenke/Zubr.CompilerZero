using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ParameterSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.Parameter;

	public SyntaxList<AttributeSyntax> Attributes { get; }

	public TokenList Modifiers { get; }

	public TypeSyntax Type { get; }

	public Token Identifier { get; }

	public EqualsValueClauseSyntax? Default { get; }

	internal ParameterSyntax(
		SyntaxTree tree,
		TextSpan span,
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		TypeSyntax type,
		Token identifier,
		EqualsValueClauseSyntax? @default
	) : base(tree, span)
	{
		Attributes = attributes;
		Modifiers = modifiers;
		Identifier = identifier;
		Type = type;
		Default = @default;

		SetParent(attributes);
		SetParent(type);
		SetParentIfNotNull(@default);
	}

	public override string ToString()
	{
		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Type} {Identifier}{(Default is null ? "" : $" {Default}")}";
	}
}
