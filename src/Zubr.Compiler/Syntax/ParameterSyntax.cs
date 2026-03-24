using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ParameterSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.Parameter;

	public SyntaxList<AttributeSyntax> Attributes { get; }

	public TokenList Modifiers { get; }

	public VariadicSpecifierSyntax? Variadic { get; }

	public TypeSyntax Type { get; }

	public Token Identifier { get; }

	public EqualsValueClauseSyntax? Default { get; }

	internal ParameterSyntax(
		SyntaxTree tree,
		TextSpan span,
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		VariadicSpecifierSyntax? variadic,
		TypeSyntax type,
		Token identifier,
		EqualsValueClauseSyntax? @default
	) : base(tree, span)
	{
		Attributes = attributes;
		Modifiers = modifiers;
		Variadic = variadic;
		Type = type;
		Identifier = identifier;
		Default = @default;

		SetParent(attributes);
		SetParentIfNotNull(variadic);
		SetParent(type);
		SetParentIfNotNull(@default);
	}

	public override string ToString()
	{
		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{(Variadic is null ? "" : $"{Variadic} ")}{Type} {Identifier}{(Default is null ? "" : $" {Default}")}";
	}
}
