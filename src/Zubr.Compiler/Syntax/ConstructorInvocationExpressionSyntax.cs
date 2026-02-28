using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ConstructorInvocationExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind { get; }

	public Token Keyword { get; }

	public ArgumentListSyntax ArgumentList { get; }

	internal ConstructorInvocationExpressionSyntax(SyntaxTree tree, TextSpan span, SyntaxKind kind,  Token keyword, ArgumentListSyntax argumentList) : base(tree, span)
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
