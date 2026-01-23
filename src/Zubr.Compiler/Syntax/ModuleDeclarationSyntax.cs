using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ModuleDeclarationSyntax : MemberDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ModuleKeyword;

	public override SyntaxTokenList Modifiers => SyntaxTokenList.Empty;

	public SyntaxToken ModuleKeyword { get; }

	public SyntaxToken TopKeyword { get; }

	public NameSyntax? Name { get; }

	public SyntaxToken SemicolonToken { get; }

	public SyntaxList<MemberDeclarationSyntax> Members { get; }

	internal ModuleDeclarationSyntax(SyntaxToken moduleKeyword, SyntaxToken topKeyword, NameSyntax? name, SyntaxToken semicolonToken, SyntaxList<MemberDeclarationSyntax> members)
	{
		ModuleKeyword = moduleKeyword;
		TopKeyword = topKeyword;
		Name = name;
		SemicolonToken = semicolonToken;
		Members = members;

		SetParentIfNotNull(name);
		SetParent(members);
	}
}
