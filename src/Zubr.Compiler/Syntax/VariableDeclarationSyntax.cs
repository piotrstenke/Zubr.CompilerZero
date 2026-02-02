using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class VariableDeclarationSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;

	public TypeSyntax Type { get; }

	public Token Identifier { get; }

	public EqualsValueClauseSyntax? Initializer { get; }

	internal VariableDeclarationSyntax(TypeSyntax type, Token identifier, EqualsValueClauseSyntax? initializer)
	{
		Type = type;
		Identifier = identifier;
		Initializer = initializer;

		SetParent(type);
		SetParentIfNotNull(initializer);
	}

	public override string ToString()
	{
		return $"{Type} {Identifier}{(Initializer is null ? "" : $" {Initializer}")}";
	}
}
