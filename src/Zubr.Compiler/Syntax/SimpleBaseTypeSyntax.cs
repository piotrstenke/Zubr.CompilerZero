using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class SimpleBaseTypeSyntax : BaseTypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.SimpleBaseType;

	public override TypeSyntax Type { get; }

	internal SimpleBaseTypeSyntax(TypeSyntax type)
	{
		Type = type;

		SetParent(type);
	}

	public override string ToString()
	{
		return $"{Type}";
	}
}
