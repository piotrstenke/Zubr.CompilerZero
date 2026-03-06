using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class UnionTypeSyntax : TypeSyntax
{
	public override SyntaxKind Kind => SyntaxKind.UnionType;

	public SeparatedSyntaxList<TypeSyntax> ElementTypes { get; }

	internal UnionTypeSyntax(SyntaxTree tree, TextSpan span, SeparatedSyntaxList<TypeSyntax> elementTypes) : base(tree, span)
	{
		ElementTypes = elementTypes;

		SetParent(elementTypes);
	}

	public override string ToString()
	{
		return $"{ElementTypes}";
	}
}
