using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class VariableDeclarationSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;

	public TypeSyntax Type { get; }

	public VariableDeclaratorSyntax Variable { get; }

	internal VariableDeclarationSyntax(TypeSyntax type, VariableDeclaratorSyntax variable)
	{
		Type = type;
		Variable = variable;

		SetParent(type);
		SetParent(variable);
	}
}
