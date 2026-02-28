using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class GotoStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind =>  SyntaxKind.GotoStatement;

	public Token Keyword { get; }

	public Token Identifier { get; }

	public Token SemicolonToken { get; }

	internal GotoStatementSyntax(SyntaxTree tree, TextSpan span, Token keyword, Token identifier, Token semicolonToken) : base(tree, span)
	{
		Keyword = keyword;
		Identifier = identifier;
		SemicolonToken = semicolonToken;
	}

	public override string ToString()
	{
		return $"{Keyword} {Identifier}{SemicolonToken}";
	}
}
