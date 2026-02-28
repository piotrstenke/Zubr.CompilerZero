using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class IdentifierNameSyntax : SimpleNameSyntax
{
	public override SyntaxKind Kind => SyntaxKind.IdentifierName;

	public override Token Identifier { get; }

	internal IdentifierNameSyntax(SyntaxTree tree, TextSpan span, Token identifier) : base(tree, span)
	{
		Identifier = identifier;
	}

	public override string ToString()
	{
		return $"{Identifier}";
	}
}
