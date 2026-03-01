using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class SimpleUseDirectiveSyntax : UseDirectiveSyntax
{
	public override SyntaxKind Kind => SyntaxKind.SimpleUseDirective;

	public override Token UseKeyword { get; }

	public NameSyntax Name { get; }

	public Token AsKeyword { get; }

	public IdentifierNameSyntax? Alias { get; }

	public override Token SemicolonToken { get; }

	internal SimpleUseDirectiveSyntax(
		SyntaxTree tree,
		TextSpan span,
		Token useKeyword,
		NameSyntax name,
		Token asKeyword,
		IdentifierNameSyntax? alias,
		Token semicolonToken
	) : base(tree, span)
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
		if (Alias is null)
		{
			return $"{UseKeyword} {Name}{SemicolonToken}";
		}

		return $"{UseKeyword} {Name} {AsKeyword} {Alias}{SemicolonToken}";
	}
}
