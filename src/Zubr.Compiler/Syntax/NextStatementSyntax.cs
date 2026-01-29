using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class NextStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.NextStatement;

	public Token NextKeyword { get; }

	public Token SemicolonToken { get; }

	internal NextStatementSyntax(Token nextKeyword, Token semicolonToken)
	{
		NextKeyword = nextKeyword;
		SemicolonToken = semicolonToken;
	}

	public override string ToString()
	{
		return $"{NextKeyword}{SemicolonToken}";
	}
}
