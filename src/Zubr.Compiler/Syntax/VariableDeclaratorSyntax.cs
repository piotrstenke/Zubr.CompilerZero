namespace Zubr.Compiler.Syntax;

public sealed class VariableDeclaratorSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.VariableDeclarator;

	public SyntaxToken Identifier { get; }

	public EqualsValueClauseSyntax Initializer { get; }

	internal VariableDeclaratorSyntax(SyntaxToken identifier, EqualsValueClauseSyntax initializer)
	{
		Identifier = identifier;
		Initializer = initializer;

		SetParent(initializer);
	}

	public override string ToString()
	{
		return $"{Identifier} {Initializer}";
	}
}
