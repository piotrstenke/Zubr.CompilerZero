namespace Zubr.Compiler.Syntax;

public sealed class TypeParameterSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeParameter;

	public SyntaxToken Identifier { get; }

	internal TypeParameterSyntax(SyntaxToken identifier)
	{
		Identifier = identifier;
	}

	public override string ToString()
	{
		return $"{Identifier}";
	}
}
