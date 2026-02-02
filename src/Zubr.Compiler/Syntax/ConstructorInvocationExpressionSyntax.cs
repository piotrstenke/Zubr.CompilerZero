using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ConstructorInvocationExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind { get; }

	public Token Keyword { get; }

	public ArgumentListSyntax ArgumentList { get; }

	internal ConstructorInvocationExpressionSyntax(SyntaxKind kind,  Token keyword, ArgumentListSyntax argumentList)
	{
		Kind = kind;
		Keyword = keyword;
		ArgumentList = argumentList;

		SetParent(argumentList);
	}

	public override string ToString()
	{
		return $"{Keyword}{ArgumentList}";
	}
}
