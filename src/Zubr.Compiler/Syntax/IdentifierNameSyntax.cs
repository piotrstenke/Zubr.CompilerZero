using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class IdentifierNameSyntax : SimpleNameSyntax
{
	public override SyntaxKind Kind => SyntaxKind.IdentifierName;

	public override SyntaxToken Identifier { get; }

	internal IdentifierNameSyntax(SyntaxToken identifier)
	{
		Identifier = identifier;
	}

	public override string ToString()
	{
		return $"{Identifier}";
	}
}
