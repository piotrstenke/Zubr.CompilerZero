using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class IfStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.IfStatement;

	public Token IfKeyword { get; }

	public Token OpenParenToken { get; }

	public ExpressionSyntax Condition { get; }

	public StatementSyntax Statement { get; }

	public Token CloseParenToken { get; }

	public SyntaxList<ElifClauseSyntax> Elifs { get; }

	public ElseClauseSyntax? Else { get; }

	internal IfStatementSyntax(SyntaxTree tree, TextSpan span, Token ifKeyword, Token openParenToken, ExpressionSyntax condition, Token closeParenToken, StatementSyntax statement, SyntaxList<ElifClauseSyntax> elifs, ElseClauseSyntax? @else) : base(tree, span)
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
