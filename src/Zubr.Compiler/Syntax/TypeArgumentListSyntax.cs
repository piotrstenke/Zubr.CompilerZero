using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class TypeArgumentListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeArgumentList;

	public Token LessThanToken { get; }

	public SeparatedSyntaxList<TypeSyntax> Arguments { get; }

	public Token GreaterThanToken { get; }

	internal TypeArgumentListSyntax(SyntaxTree tree, TextSpan span, Token lessThanToken, SeparatedSyntaxList<TypeSyntax> arguments, Token greaterThanToken) : base(tree, span)
	{
		LessThanToken = lessThanToken;
		Arguments = arguments;
		GreaterThanToken = greaterThanToken;

		SetParent(arguments);
	}

	public override string ToString()
	{
		return $"{LessThanToken}{Arguments}{GreaterThanToken}";
	}
}
