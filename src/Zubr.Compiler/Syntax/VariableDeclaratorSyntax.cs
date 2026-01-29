namespace Zubr.Compiler.Syntax;

public sealed class VariableDeclaratorSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.VariableDeclarator;

	public Token Identifier { get; }

	public EqualsValueClauseSyntax Initializer { get; }

	internal VariableDeclaratorSyntax(Token identifier, EqualsValueClauseSyntax initializer)
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
