using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class TypeParameterListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeParameterList;

	public SyntaxToken LessThanToken { get; }

	public SeparatedSyntaxList<TypeParameterSyntax> Parameters { get; }

	public SyntaxToken GreaterThanToken { get; }

	internal TypeParameterListSyntax(SyntaxToken lessThanToken, SeparatedSyntaxList<TypeParameterSyntax> parameters, SyntaxToken greaterThanToken)
	{
		LessThanToken = lessThanToken;
		Parameters = parameters;
		GreaterThanToken = greaterThanToken;

		SetParent(parameters);
	}

	public override string ToString()
	{
		return $"{LessThanToken}{Parameters}{GreaterThanToken}";
	}
}
