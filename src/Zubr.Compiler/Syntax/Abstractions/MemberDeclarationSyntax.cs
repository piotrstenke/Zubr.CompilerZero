using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class MemberDeclarationSyntax : NameSyntax
{
	public abstract SyntaxList<AttributeSyntax> Attributes { get; }

	public abstract TokenList Modifiers { get; }

	internal MemberDeclarationSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{
	}
}
