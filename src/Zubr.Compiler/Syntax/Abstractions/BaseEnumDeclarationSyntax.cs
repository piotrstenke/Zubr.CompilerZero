using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class BaseEnumDeclarationSyntax : BaseTypeDeclarationSyntax
{
	public abstract SeparatedSyntaxList<EnumMemberDeclarationSyntax> Members { get; }

	internal BaseEnumDeclarationSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{
	}
}
