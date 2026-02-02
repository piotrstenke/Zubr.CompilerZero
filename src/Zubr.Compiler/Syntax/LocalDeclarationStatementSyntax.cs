using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class LocalDeclarationStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.LocalDeclarationStatement;

	public TokenList Modifiers { get; }

	public VariableDeclarationSyntax Variable { get; }

	public Token SemicolonToken { get; }

	internal LocalDeclarationStatementSyntax(TokenList modifiers, VariableDeclarationSyntax variable, Token semicolonToken)
	{
		Modifiers = modifiers;
		Variable = variable;
		SemicolonToken = semicolonToken;

		SetParent(variable);
	}

	public override string ToString()
	{
		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Variable}{SemicolonToken}";
	}
}
