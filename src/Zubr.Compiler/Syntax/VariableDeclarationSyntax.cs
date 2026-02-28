using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class VariableDeclarationSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;

	public TypeSyntax Type { get; }

	public Token Identifier { get; }

	public EqualsValueClauseSyntax? Initializer { get; }

	internal VariableDeclarationSyntax(SyntaxTree tree, TextSpan span, TypeSyntax type, Token identifier, EqualsValueClauseSyntax? initializer) : base(tree, span)
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
