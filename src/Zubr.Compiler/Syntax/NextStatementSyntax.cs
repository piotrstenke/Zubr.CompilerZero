using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class NextStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.NextStatement;

	public Token NextKeyword { get; }

	public Token SemicolonToken { get; }

	internal NextStatementSyntax(SyntaxTree tree, TextSpan span, Token nextKeyword, Token semicolonToken) : base(tree, span)
	{
		NextKeyword = nextKeyword;
		SemicolonToken = semicolonToken;
	}

	public override string ToString()
	{
		return $"{NextKeyword}{SemicolonToken}";
	}
}
