using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class BlockSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.Block;

	public SyntaxToken OpenBraceToken { get; }

	public SyntaxList<StatementSyntax> Statements { get; }

	public SyntaxToken CloseBraceToken { get; }

	internal BlockSyntax(SyntaxToken openBraceToken, SyntaxList<StatementSyntax> statements, SyntaxToken closeBraceToken)
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
