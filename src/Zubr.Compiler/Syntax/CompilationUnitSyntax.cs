using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class CompilationUnitSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

	public SyntaxList<UseDirectiveSyntax> Uses { get; }

	public SyntaxList<AliasDirectiveSyntax> Aliases { get; }

	public SyntaxList<MemberDeclarationSyntax> Members { get; }

	public Token EndOfFileToken { get; }

	internal CompilationUnitSyntax(
		SyntaxList<UseDirectiveSyntax> uses,
		SyntaxList<AliasDirectiveSyntax> aliases,
		SyntaxList<MemberDeclarationSyntax> members,
		Token endOfFileToken
	)
	{
		Uses = uses;
		Aliases = aliases;
		Members = members;
		EndOfFileToken = endOfFileToken;

		SetParent(uses);
		SetParent(aliases);
		SetParent(members);
	}
}
