using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class TypeArgumentListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeArgumentList;

	public SyntaxToken LessThanToken { get; }

	public SeparatedSyntaxList<TypeSyntax> Arguments { get; }

	public SyntaxToken GreaterThanToken { get; }

	internal TypeArgumentListSyntax(SyntaxToken lessThanToken, SeparatedSyntaxList<TypeSyntax> arguments, SyntaxToken greaterThanToken)
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
