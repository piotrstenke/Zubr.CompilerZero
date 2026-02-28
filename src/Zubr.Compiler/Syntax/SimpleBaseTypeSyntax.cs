using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class SimpleBaseTypeSyntax : BaseTypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.SimpleBaseType;

	public override TypeSyntax Type { get; }

	internal SimpleBaseTypeSyntax(SyntaxTree tree, TextSpan span, TypeSyntax type) : base(tree, span)
	{
		Type = type;

		SetParent(type);
	}

	public override string ToString()
	{
		return $"{Type}";
	}
}
