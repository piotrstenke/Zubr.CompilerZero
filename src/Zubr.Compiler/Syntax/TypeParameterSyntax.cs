using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class TypeParameterSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeParameter;

	public Token Identifier { get; }

	internal TypeParameterSyntax(SyntaxTree tree, TextSpan span, Token identifier) : base(tree, span)
	{
		Identifier = identifier;
	}

	public override string ToString()
	{
		return $"{Identifier}";
	}
}
