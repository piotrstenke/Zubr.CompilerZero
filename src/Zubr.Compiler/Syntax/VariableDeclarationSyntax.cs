using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class VariableDeclarationSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;

	public TypeSyntax Type { get; }

	public SeparatedSyntaxList<VariableDeclaratorSyntax> Variables { get; }

	internal VariableDeclarationSyntax(SyntaxTree tree, TextSpan span, TypeSyntax type, SeparatedSyntaxList<VariableDeclaratorSyntax> variables) : base(tree, span)
	{
		Type = type;
		Variables = variables;

		SetParent(type);
		SetParent(variables);
	}

	public override string ToString()
	{
		return $"{Type} {Variables}";
	}
}
