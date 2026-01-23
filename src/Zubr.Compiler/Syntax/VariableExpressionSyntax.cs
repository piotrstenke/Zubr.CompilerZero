using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class VariableExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.VariableExpression;

	public TypeSyntax? Type { get; }

	public SyntaxToken Identifier { get; }

	internal VariableExpressionSyntax(TypeSyntax? type, SyntaxToken identifier)
	{
		Type = type;
		Identifier = identifier;

		SetParentIfNotNull(type);
	}
}
