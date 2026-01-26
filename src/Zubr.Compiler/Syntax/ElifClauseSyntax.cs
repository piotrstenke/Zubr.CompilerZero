using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ElifClauseSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ElifClause;

	public SyntaxToken ElifKeyword { get; }

	public SyntaxToken OpenParenToken { get; }

	public ExpressionSyntax Condition { get; }

	public SyntaxToken CloseParenToken { get; }

	public StatementSyntax Statement { get; }

	internal ElifClauseSyntax(SyntaxToken elifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		ElifKeyword = elifKeyword;
		OpenParenToken = openParenToken;
		Condition = condition;
		Statement = statement;
		CloseParenToken = closeParenToken;

		SetParent(condition);
		SetParent(statement);
	}

	public override string ToString()
	{
		return $"{ElifKeyword} {OpenParenToken}{Condition}{CloseParenToken} {Statement}";
	}
}
