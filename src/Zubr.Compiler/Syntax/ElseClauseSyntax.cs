using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ElseClauseSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ElseClause;

	public SyntaxToken ElseKeyword { get; }

	public StatementSyntax Statement { get; }

	internal ElseClauseSyntax(SyntaxToken elseKeyword, StatementSyntax statement)
	{
		ElseKeyword = elseKeyword;
		Statement = statement;

		SetParent(statement);
	}
}
