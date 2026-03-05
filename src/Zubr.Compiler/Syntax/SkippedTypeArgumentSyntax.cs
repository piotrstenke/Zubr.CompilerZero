using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class SkippedTypeArgumentSyntax : TypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.SkippedTypeArgument;

	public Token Token { get; }

	internal SkippedTypeArgumentSyntax(SyntaxTree tree, TextSpan span, Token token) : base(tree, span)
	{
		Token = token;
	}

	public override string ToString()
	{
		return string.Empty;
	}
}
