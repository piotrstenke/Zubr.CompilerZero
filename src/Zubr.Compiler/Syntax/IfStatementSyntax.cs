using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class IfStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.IfStatement;

	public SyntaxToken IfKeyword { get; }

	public SyntaxToken OpenParenToken { get; }

	public ExpressionSyntax Condition { get; }

	public StatementSyntax Statement { get; }

	public SyntaxToken CloseParenToken { get; }

	public SyntaxList<ElifClauseSyntax> Elifs { get; }

	public ElseClauseSyntax? Else { get; }

	internal IfStatementSyntax(SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, SyntaxList<ElifClauseSyntax> elifs, ElseClauseSyntax? @else)
	{
		IfKeyword = ifKeyword;
		OpenParenToken = openParenToken;
		Condition = condition;
		Statement = statement;
		CloseParenToken = closeParenToken;
		Elifs = elifs;
		Else = @else;

		SetParent(condition);
		SetParent(statement);
		SetParent(elifs);
		SetParentIfNotNull(@else);
	}

	public override string ToString()
	{
		if (Else is null && Elifs.IsDefaultOrEmpty)
		{
			return $"{IfKeyword} {OpenParenToken}{Condition}{CloseParenToken} {Statement}";
		}

		return $"{IfKeyword} {OpenParenToken}{Condition}{CloseParenToken} {Statement} ...";
	}
}
