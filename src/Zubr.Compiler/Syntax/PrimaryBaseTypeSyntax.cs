using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class PrimaryBaseTypeSyntax : BaseTypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.PrimaryBaseType;

	public override TypeSyntax Type { get; }

	public ArgumentListSyntax ArgumentList { get; }

	internal PrimaryBaseTypeSyntax(SyntaxTree tree, TextSpan span, TypeSyntax type, ArgumentListSyntax argumentList) : base(tree, span)
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
