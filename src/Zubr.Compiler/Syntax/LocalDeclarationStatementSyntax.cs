using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class LocalDeclarationStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.LocalDeclarationStatement;

	public TokenList Modifiers { get; }

	public VariableDeclarationSyntax Variable { get; }

	public Token SemicolonToken { get; }

	internal LocalDeclarationStatementSyntax(SyntaxTree tree, TextSpan span, TokenList modifiers, VariableDeclarationSyntax variable, Token semicolonToken) : base(tree, span)
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
