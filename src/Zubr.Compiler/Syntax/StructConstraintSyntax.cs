using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class StructConstraintSyntax : TypeParameterConstraintSyntax
{
	public override SyntaxKind Kind => SyntaxKind.StructConstraint;

	public Token StructKeyword { get; }

	internal StructConstraintSyntax(Token structKeyword)
	{
		StructKeyword = structKeyword;
	}

	public override string ToString()
	{
		return $"{StructKeyword}";
	}
}
