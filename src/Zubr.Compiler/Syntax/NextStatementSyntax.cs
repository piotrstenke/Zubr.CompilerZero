using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class NextStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.NextStatement;

	public SyntaxToken NextKeyword { get; }

	public SyntaxToken SemicolonToken { get; }

	internal NextStatementSyntax(SyntaxToken nextKeyword, SyntaxToken semicolonToken)
	{
		NextKeyword = nextKeyword;
		SemicolonToken = semicolonToken;
	}

	public override string ToString()
	{
		return $"{NextKeyword}{SemicolonToken}";
	}
}
