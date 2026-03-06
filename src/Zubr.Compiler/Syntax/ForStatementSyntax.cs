using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ForStatementSyntax : BaseForStatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ForStatement;

	public override Token ForKeyword { get; }

	public override Token OpenParenToken { get; }

	public VariableDeclarationSyntax? Declaration { get; }

	public SeparatedSyntaxList<ExpressionSyntax> Initializers { get; }

	public Token FirstSemicolonToken { get; }

	public ExpressionSyntax? Condition { get; }

	public Token SecondSemicolonToken { get; }

	public SeparatedSyntaxList<ExpressionSyntax> Incrementors { get; }

	public override Token CloseParenToken { get; }

	public override StatementSyntax Statement { get; }

	internal ForStatementSyntax(
		SyntaxTree tree,
		TextSpan span,
		Token forKeyword,
		Token openParenToken,
		VariableDeclarationSyntax? declaration,
		SeparatedSyntaxList<ExpressionSyntax> initializers,
		Token firstSemicolonToken,
		ExpressionSyntax? condition,
		Token secondSemicolonToken,
		SeparatedSyntaxList<ExpressionSyntax> incrementors,
		Token closeParenToken,
		StatementSyntax statement
	) : base(tree, span)
	{
		ForKeyword = forKeyword;
		OpenParenToken = openParenToken;
		Declaration = declaration;
		Initializers = initializers;
		FirstSemicolonToken = firstSemicolonToken;
		Condition = condition;
		SecondSemicolonToken = secondSemicolonToken;
		Incrementors = incrementors;
		CloseParenToken = closeParenToken;
		Statement = statement;

		SetParentIfNotNull(declaration);
		SetParent(initializers);
		SetParentIfNotNull(condition);
		SetParent(incrementors);
		SetParent(statement);
	}

	public override string ToString()
	{
		return $"{ForKeyword} {OpenParenToken}{Declaration}{Initializers}{FirstSemicolonToken} {Condition}{SecondSemicolonToken} {Incrementors}{CloseParenToken} {Statement}";
	}
}
