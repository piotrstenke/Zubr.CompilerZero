using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ListedUseDirectiveSyntax : UseDirectiveSyntax
{
	public override SyntaxKind Kind => SyntaxKind.ListedUseDirective;

	public override Token UseKeyword { get; }

	public UseDirectiveElementListSyntax ElementList { get; }

	public Token FromKeyword { get; }

	public NameSyntax Module { get; }

	public override Token SemicolonToken { get; }

	internal ListedUseDirectiveSyntax(
		SyntaxTree tree,
		TextSpan span,
		Token useKeyword,
		UseDirectiveElementListSyntax elementList,
		Token fromKeyword,
		NameSyntax module,
		Token semicolonToken
	) : base(tree, span)
	{
		UseKeyword = useKeyword;
		ElementList = elementList;
		FromKeyword = fromKeyword;
		Module = module;
		SemicolonToken = semicolonToken;

		SetParent(elementList);
		SetParent(module);
	}

	public override string ToString()
	{
		return $"{UseKeyword} {ElementList} {FromKeyword} {Module}{SemicolonToken}";
	}
}
