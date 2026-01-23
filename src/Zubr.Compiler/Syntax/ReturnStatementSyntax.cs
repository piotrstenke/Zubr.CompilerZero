using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ReturnStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ReturnStatement;

	public SyntaxToken ReturnKeyword { get; }

	public ExpressionSyntax? Expression { get; }

	public SyntaxToken SemicolonToken { get; }

	internal ReturnStatementSyntax(SyntaxToken returnKeyword, ExpressionSyntax? expression, SyntaxToken semicolonToken)
	{
		ReturnKeyword = returnKeyword;
		Expression = expression;
		SemicolonToken = semicolonToken;

		SetParentIfNotNull(expression);
	}
}
