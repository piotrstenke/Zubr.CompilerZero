using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class UseDirectiveSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.UseDirective;

	public SyntaxToken UseKeyword { get; }

	public NameSyntax Name { get; }

	public SyntaxToken AsKeyword { get; }

	public IdentifierNameSyntax? Alias { get; }

	public SyntaxToken SemicolonToken { get; }

	internal UseDirectiveSyntax(SyntaxToken useKeyword, NameSyntax name, SyntaxToken asKeyword, IdentifierNameSyntax? alias, SyntaxToken semicolonToken)
	{
		UseKeyword = useKeyword;
		Name = name;
		AsKeyword = asKeyword;
		Alias = alias;
		SemicolonToken = semicolonToken;

		SetParent(name);
		SetParentIfNotNull(alias);
	}

	public override string ToString()
	{
		if (Alias is not null)
		{
			return $"{UseKeyword} {Name} {AsKeyword} {Alias}{SemicolonToken}";
		}

		return $"{UseKeyword} {Name}{SemicolonToken}";
	}
}
