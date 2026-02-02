using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class ModuleDeclarationSyntax : MemberDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ModuleDeclaration;

	public override TokenList Modifiers => TokenList.Empty;

	public override SyntaxList<AttributeSyntax> Attributes => default;

	public Token ModuleKeyword { get; }

	public Token TopKeyword { get; }

	public NameSyntax? Name { get; }

	public Token SemicolonToken { get; }

	public SyntaxList<MemberDeclarationSyntax> Members { get; }

	internal ModuleDeclarationSyntax(Token moduleKeyword, Token topKeyword, NameSyntax? name, Token semicolonToken, SyntaxList<MemberDeclarationSyntax> members)
	{
		ModuleKeyword = moduleKeyword;
		TopKeyword = topKeyword;
		Name = name;
		SemicolonToken = semicolonToken;
		Members = members;

		SetParentIfNotNull(name);
		SetParent(members);
	}

	public override string ToString()
	{
		if (Name is null)
		{
			return $"{ModuleKeyword} {TopKeyword}{SemicolonToken}";
		}

		return $"{ModuleKeyword} {Name}{SemicolonToken}";
	}
}
