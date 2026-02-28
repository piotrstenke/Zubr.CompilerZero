using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class TypeDeclarationSyntax : BaseTypeDeclarationSyntax
{
	public abstract TypeParameterListSyntax? TypeParameterList { get; }

	public abstract ParameterListSyntax? ParameterList { get; }

	public abstract TypeParameterConstraintListSyntax? ConstraintList { get; }

	public abstract SyntaxList<MemberDeclarationSyntax> Members { get; }

	internal TypeDeclarationSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{
	}
}
