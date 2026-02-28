using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ElifClauseSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ElifClause;

	public Token ElifKeyword { get; }

	public Token OpenParenToken { get; }

	public ExpressionSyntax Condition { get; }

	public Token CloseParenToken { get; }

	public StatementSyntax Statement { get; }

	internal ElifClauseSyntax(SyntaxTree tree, TextSpan span, Token elifKeyword, Token openParenToken, ExpressionSyntax condition, Token closeParenToken, StatementSyntax statement) : base(tree, span)
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
