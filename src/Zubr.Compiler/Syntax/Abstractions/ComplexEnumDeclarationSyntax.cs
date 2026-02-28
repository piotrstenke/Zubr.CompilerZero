using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class ComplexEnumDeclarationSyntax : BaseEnumDeclarationSyntax
{
	public abstract ParameterListSyntax? ParameterList { get; }

	internal ComplexEnumDeclarationSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{
	}
}
