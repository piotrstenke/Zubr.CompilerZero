using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class BlockSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.Block;

	public Token OpenBraceToken { get; }

	public SyntaxList<StatementSyntax> Statements { get; }

	public Token CloseBraceToken { get; }

	internal BlockSyntax(SyntaxTree tree, TextSpan span, Token openBraceToken, SyntaxList<StatementSyntax> statements, Token closeBraceToken) : base(tree, span)
	{
		OpenBraceToken = openBraceToken;
		Statements = statements;
		CloseBraceToken = closeBraceToken;

		SetParent(statements);
	}

	public override string ToString()
	{
		return $"{OpenBraceToken} ... {CloseBraceToken}";
	}
}
