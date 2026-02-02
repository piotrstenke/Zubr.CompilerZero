using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class PrimaryBaseTypeSyntax : BaseTypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.PrimaryBaseType;

	public override TypeSyntax Type { get; }

	public ArgumentListSyntax ArgumentList { get; }

	internal PrimaryBaseTypeSyntax(TypeSyntax type, ArgumentListSyntax argumentList)
	{
		Type = type;
		ArgumentList = argumentList;

		SetParent(type);
		SetParent(argumentList);
	}

	public override string ToString()
	{
		return $"{Type}{ArgumentList}";
	}
}
