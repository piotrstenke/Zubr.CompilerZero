using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class LocalDeclarationStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.LocalDeclarationStatement;

	public SyntaxTokenList Modifiers { get; }

	public VariableDeclarationSyntax Declaration { get; }

	public SyntaxToken SemicolonToken { get; }

	internal LocalDeclarationStatementSyntax(SyntaxTokenList modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		Modifiers = modifiers;
		Declaration = declaration;
		SemicolonToken = semicolonToken;

		SetParent(declaration);
	}

	public override string ToString()
	{
		return $"{Modifiers} {Declaration}{SemicolonToken}";
	}
}
