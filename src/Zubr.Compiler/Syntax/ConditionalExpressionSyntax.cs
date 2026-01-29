using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ConditionalExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ConditionalExpression;

	public ExpressionSyntax Condition { get; }

	public Token QuestionToken { get; }

	public ExpressionSyntax TrueExpression { get; }

	public Token ColonToken { get; }

	public ExpressionSyntax FalseExpression { get; }

	internal ConditionalExpressionSyntax(ExpressionSyntax condition, Token questionToken, ExpressionSyntax trueExpression, Token colonToken, ExpressionSyntax falseExpression)
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
