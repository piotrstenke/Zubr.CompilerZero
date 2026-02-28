using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class UseDirectiveSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.UseDirective;

	public Token UseKeyword { get; }

	public NameSyntax Name { get; }

	public Token AsKeyword { get; }

	public IdentifierNameSyntax? Alias { get; }

	public Token FromKeyword { get; }

	public NameSyntax? ModuleName { get; }

	public Token SemicolonToken { get; }

	internal UseDirectiveSyntax(
		SyntaxTree tree,
		TextSpan span,
		Token useKeyword,
		NameSyntax name,
		Token asKeyword,
		IdentifierNameSyntax? alias,
		Token fromKeyword,
		NameSyntax? moduleName,
		Token semicolonToken
	) : base(tree, span)
	{
		UseKeyword = useKeyword;
		Name = name;
		AsKeyword = asKeyword;
		Alias = alias;
		FromKeyword = fromKeyword;
		ModuleName = moduleName;
		SemicolonToken = semicolonToken;

		SetParent(name);
		SetParentIfNotNull(alias);
		SetParentIfNotNull(moduleName);
	}

	public override string ToString()
	{
		if (Alias is null)
		{
			if(ModuleName is null)
			{
				return $"{UseKeyword} {Name}{SemicolonToken}";
			}

			return $"{UseKeyword} {Name} {FromKeyword} {ModuleName}{SemicolonToken}";
		}

		if(ModuleName is null)
		{
			return $"{UseKeyword} {Name} {AsKeyword} {Alias}{SemicolonToken}";
		}

		return $"{UseKeyword} {Name} {AsKeyword} {Alias} {FromKeyword} {ModuleName}{SemicolonToken}";
	}
}
