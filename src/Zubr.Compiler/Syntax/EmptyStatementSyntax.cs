using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class EmptyStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.EmptyStatement;

	public Token SemicolonToken { get; }

	internal EmptyStatementSyntax(SyntaxTree tree, TextSpan span, Token semicolonToken) : base(tree, span)
	{
		SemicolonToken = semicolonToken;
	}

	public override string ToString()
	{
		return $"{SemicolonToken}";
	}
}
