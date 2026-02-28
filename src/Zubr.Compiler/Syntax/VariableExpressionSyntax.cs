using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class VariableExpressionSyntax : ExpressionSyntax
{
	public override SyntaxKind Kind => SyntaxKind.VariableExpression;

	public TypeSyntax? Type { get; }

	public Token Identifier { get; }

	internal VariableExpressionSyntax(SyntaxTree tree, TextSpan span, TypeSyntax? type, Token identifier) : base(tree, span)
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
