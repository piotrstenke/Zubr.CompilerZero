using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class CompilationUnitSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

	public SyntaxList<UseDirectiveSyntax> Uses { get; }

	public SyntaxList<MemberDeclarationSyntax> Members { get; }

	public SyntaxToken EndOfFileToken { get; }

	internal CompilationUnitSyntax(SyntaxList<UseDirectiveSyntax> uses, SyntaxList<MemberDeclarationSyntax> members, SyntaxToken endOfFileToken)
	{
		Uses = uses;
		Members = members;
		EndOfFileToken = endOfFileToken;

		SetParent(uses);
		SetParent(members);
	}
}
