using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class TupleExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.TupleExpression;

	public Token OpenParenToken { get; }

	public SeparatedSyntaxList<ArgumentSyntax> Arguments { get; }

	public Token CloseParenToken { get; }

	internal TupleExpressionSyntax(SyntaxTree tree, TextSpan span, Token openParenToken, SeparatedSyntaxList<ArgumentSyntax> arguments, Token closeParenToken) : base(tree, span)
	{
		OpenParenToken = openParenToken;
		Arguments = arguments;
		CloseParenToken = closeParenToken;

		SetParent(arguments);
	}
}

