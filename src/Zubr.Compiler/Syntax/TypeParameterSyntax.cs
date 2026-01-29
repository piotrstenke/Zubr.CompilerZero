namespace Zubr.Compiler.Syntax;

public sealed class TypeParameterSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeParameter;

	public Token Identifier { get; }

	internal TypeParameterSyntax(Token identifier)
	{
		Identifier = identifier;
	}

	public override string ToString()
	{
		return $"{Identifier}";
	}
}
