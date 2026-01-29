using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ElseClauseSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ElseClause;

	public Token ElseKeyword { get; }

	public StatementSyntax Statement { get; }

	internal ElseClauseSyntax(Token elseKeyword, StatementSyntax statement)
	{
		ElseKeyword = elseKeyword;
		Statement = statement;

		SetParent(statement);
	}

	public override string ToString()
	{
		return $"{ElseKeyword} {Statement}";
	}
}
