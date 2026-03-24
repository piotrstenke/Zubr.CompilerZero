using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class StackallocExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.StackallocExpression;

	public Token StackallocKeyword { get; }

	public TypeSyntax Type { get; }

	public InitializerExpressionSyntax? Initializer { get; }

	internal StackallocExpressionSyntax(SyntaxTree tree, TextSpan span, Token stackallocKeyword, TypeSyntax type, InitializerExpressionSyntax? initializer) : base(tree, span)
	{
		StackallocKeyword = stackallocKeyword;
		Type = type;
		Initializer = initializer;

		SetParent(type);
		SetParentIfNotNull(initializer);
	}

	public override string ToString()
	{
		return $"{StackallocKeyword} {Type}{(Initializer is null ? "" : $" {Initializer}")}";
	}
}
