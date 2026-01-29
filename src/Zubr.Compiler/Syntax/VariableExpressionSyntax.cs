using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class VariableExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.VariableExpression;

	public TypeSyntax? Type { get; }

	public Token Identifier { get; }

	internal VariableExpressionSyntax(TypeSyntax? type, Token identifier)
	{
		Type = type;
		Identifier = identifier;

		SetParentIfNotNull(type);
	}

	public override string ToString()
	{
		if (Type is null)
		{
			return $"{Identifier}";
		}

		return $"{Type} {Identifier}";
	}
}
