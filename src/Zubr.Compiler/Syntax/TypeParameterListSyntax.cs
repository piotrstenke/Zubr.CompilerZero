using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class TypeParameterListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeParameterList;

	public Token LessThanToken { get; }

	public SeparatedSyntaxList<TypeParameterSyntax> Parameters { get; }

	public Token GreaterThanToken { get; }

	internal TypeParameterListSyntax(Token lessThanToken, SeparatedSyntaxList<TypeParameterSyntax> parameters, Token greaterThanToken)
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
