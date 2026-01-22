using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ModuleDeclarationSyntax : MemberDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ModuleKeyword;

	public SyntaxToken ModuleKeyword { get; }

	public SyntaxToken TopKeyword { get; }

	public override NameSyntax? Name { get; }

	public SyntaxToken SemicolonToken { get; }

	internal ModuleDeclarationSyntax(SyntaxToken moduleKeyword, SyntaxToken topKeyword, NameSyntax? name, SyntaxToken semicolonToken)
	{
		ModuleKeyword = moduleKeyword;
		TopKeyword = topKeyword;
		Name = name;
		SemicolonToken = semicolonToken;

		SetParentIfNotNull(name);
	}
}
