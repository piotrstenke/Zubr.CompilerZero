using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class VariableDeclaratorSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.VariableDeclarator;

	public Token Identifier { get; }

	public EqualsValueClauseSyntax? Initializer { get; }

	internal VariableDeclaratorSyntax(SyntaxTree tree, TextSpan span, Token identifier, EqualsValueClauseSyntax? initializer) : base(tree, span)
	{
		Identifier = identifier;
		Initializer = initializer;

		SetParentIfNotNull(initializer);
	}

	public override string ToString()
	{
		return $"{Identifier}{(Initializer is null ? "" : $" {Initializer}")}";
	}
}
