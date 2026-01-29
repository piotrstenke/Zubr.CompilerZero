using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class LocalDeclarationStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.LocalDeclarationStatement;

	public TokenList Modifiers { get; }

	public VariableDeclarationSyntax Declaration { get; }

	public Token SemicolonToken { get; }

	internal LocalDeclarationStatementSyntax(TokenList modifiers, VariableDeclarationSyntax declaration, Token semicolonToken)
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
