using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class IdentifierNameSyntax : SimpleNameSyntax
{
	public override SyntaxKind Kind => SyntaxKind.IdentifierName;

	public override Token Identifier { get; }

	internal IdentifierNameSyntax(Token identifier)
	{
		Identifier = identifier;
	}

	public override string ToString()
	{
		return $"{Identifier}";
	}
}
