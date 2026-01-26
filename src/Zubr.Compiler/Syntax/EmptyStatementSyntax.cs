using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class EmptyStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.EmptyStatement;

	public SyntaxToken SemicolonToken { get; }

	internal EmptyStatementSyntax(SyntaxToken semicolonToken)
	{
		SemicolonToken = semicolonToken;
	}

	public override string ToString()
	{
		return $"{SemicolonToken}";
	}
}
