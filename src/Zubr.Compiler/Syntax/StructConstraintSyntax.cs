using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class StructConstraintSyntax : TypeParameterConstraintSyntax
{
	public override SyntaxKind Kind => SyntaxKind.StructConstraint;

	public Token StructKeyword { get; }

	internal StructConstraintSyntax(SyntaxTree tree, TextSpan span, Token structKeyword) : base(tree, span)
	{
		StructKeyword = structKeyword;
	}

	public override string ToString()
	{
		return $"{StructKeyword}";
	}
}
