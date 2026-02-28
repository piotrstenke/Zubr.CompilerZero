using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class CompilationUnitSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

	public SyntaxList<UseDirectiveSyntax> Uses { get; }

	public SyntaxList<AliasDirectiveSyntax> Aliases { get; }

	public SyntaxList<MemberDeclarationSyntax> Members { get; }

	public Token EndOfFileToken { get; }

	internal CompilationUnitSyntax(
		SyntaxTree tree,
		TextSpan span,
		SyntaxList<UseDirectiveSyntax> uses,
		SyntaxList<AliasDirectiveSyntax> aliases,
		SyntaxList<MemberDeclarationSyntax> members,
		Token endOfFileToken
	) : base(tree, span)
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
