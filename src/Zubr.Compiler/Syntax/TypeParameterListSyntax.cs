using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class TypeParameterListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.TypeParameterList;

	public Token LessThanToken { get; }

	public SeparatedSyntaxList<TypeParameterSyntax> Parameters { get; }

	public Token GreaterThanToken { get; }

	internal TypeParameterListSyntax(SyntaxTree tree, TextSpan span, Token lessThanToken, SeparatedSyntaxList<TypeParameterSyntax> parameters, Token greaterThanToken) : base(tree, span)
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
