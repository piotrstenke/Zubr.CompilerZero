using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ConditionalExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ConditionalExpression;

	public ExpressionSyntax Condition { get; }

	public SyntaxToken QuestionToken { get; }

	public ExpressionSyntax TrueExpression { get; }

	public SyntaxToken ColonToken { get; }

	public ExpressionSyntax FalseExpression { get; }

	internal ConditionalExpressionSyntax(ExpressionSyntax condition, SyntaxToken questionToken, ExpressionSyntax trueExpression, SyntaxToken colonToken,  ExpressionSyntax falseExpression)
	{
		Condition = condition;
		QuestionToken = questionToken;
		TrueExpression = trueExpression;
		ColonToken = colonToken;
		FalseExpression = falseExpression;

		SetParent(condition);
		SetParent(trueExpression);
		SetParent(falseExpression);
	}

	public override string ToString()
	{
		return $"{Condition} {QuestionToken} {TrueExpression} {ColonToken} {FalseExpression}";
	}
}
