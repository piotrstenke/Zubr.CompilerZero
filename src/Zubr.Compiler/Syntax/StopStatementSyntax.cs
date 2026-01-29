using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class StopStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.StopStatement;

	public Token StopKeyword { get; }

	public Token SemicolonToken { get; }

	internal StopStatementSyntax(Token stopKeyword, Token semicolonToken)
	{
		StopKeyword = stopKeyword;
		SemicolonToken = semicolonToken;
	}

	public override string ToString()
	{
		return $"{StopKeyword}{SemicolonToken}";
	}
}
