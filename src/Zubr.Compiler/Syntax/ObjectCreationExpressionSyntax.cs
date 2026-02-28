using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ObjectCreationExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ObjectCreationExpression;

	public Token NewKeyword { get; }

	public TypeSyntax? Type { get; }

	public ArgumentListSyntax? ArgumentList { get; }

	public InitializerExpressionSyntax? Initializer { get; }

	internal ObjectCreationExpressionSyntax(
		SyntaxTree tree,
		TextSpan span,
		Token newKeyword,
		TypeSyntax? type,
		ArgumentListSyntax? argumentList,
		InitializerExpressionSyntax? initializer
	) : base(tree, span)
	{
		NewKeyword = newKeyword;
		Type = type;
		ArgumentList = argumentList;
		Initializer = initializer;

		SetParentIfNotNull(type);
		SetParentIfNotNull(argumentList);
		SetParentIfNotNull(initializer);
	}

	public override string ToString()
	{
		return $"{NewKeyword}{(Type is null ? "" : $" {Type}")}{ArgumentList}{(Initializer is null ? "" : $" {Initializer}")}";
	}
}
