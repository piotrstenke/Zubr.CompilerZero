using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class StopStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.StopStatement;

	public Token StopKeyword { get; }

	public Token SemicolonToken { get; }

	internal StopStatementSyntax(SyntaxTree tree, TextSpan span, Token stopKeyword, Token semicolonToken) : base(tree, span)
	{
		StopKeyword = stopKeyword;
		SemicolonToken = semicolonToken;
	}

	public override string ToString()
	{
		return $"{StopKeyword}{SemicolonToken}";
	}
}
