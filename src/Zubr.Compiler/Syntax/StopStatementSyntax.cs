using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class StopStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.StopStatement;

	public SyntaxToken StopKeyword { get; }

	public SyntaxToken SemicolonToken { get; }

	internal StopStatementSyntax(SyntaxToken stopKeyword, SyntaxToken semicolonToken)
	{
		StopKeyword = stopKeyword;
		SemicolonToken = semicolonToken;
	}

	public override string ToString()
	{
		return $"{StopKeyword}{SemicolonToken}";
	}
}
