using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class UseDirectiveSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.UseDirective;

	public Token UseKeyword { get; }

	public NameSyntax Name { get; }

	public Token AsKeyword { get; }

	public IdentifierNameSyntax? Alias { get; }

	public Token SemicolonToken { get; }

	internal UseDirectiveSyntax(Token useKeyword, NameSyntax name, Token asKeyword, IdentifierNameSyntax? alias, Token semicolonToken)
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
