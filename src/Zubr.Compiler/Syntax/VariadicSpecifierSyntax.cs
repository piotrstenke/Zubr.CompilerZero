using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class VariadicSpecifierSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.VariadicSpecifier;

	public Token Operator { get; }

	public ExpressionSyntax? MaxValue { get; }

	internal VariadicSpecifierSyntax(SyntaxTree tree, TextSpan span, Token operatorToken, ExpressionSyntax? maxValue) : base(tree, span)
	{
		Operator = operatorToken;
		MaxValue = maxValue;

		SetParentIfNotNull(maxValue);
	}

	public override string ToString()
	{
		return $"{Operator}{(MaxValue is null ? "" : $" {MaxValue})")}";
	}
}
