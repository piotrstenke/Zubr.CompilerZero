using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class TypeArgumentListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeArgumentList;

	public Token LessThanToken { get; }

	public SeparatedSyntaxList<TypeSyntax> Arguments { get; }

	public Token GreaterThanToken { get; }

	internal TypeArgumentListSyntax(Token lessThanToken, SeparatedSyntaxList<TypeSyntax> arguments, Token greaterThanToken)
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
