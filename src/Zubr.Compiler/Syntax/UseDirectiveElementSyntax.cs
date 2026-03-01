using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class UseDirectiveElementSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.UseDirectiveElement;

	public SimpleNameSyntax Name { get; }

	public Token AsKeyword { get; }

	public IdentifierNameSyntax? Alias { get; }

	internal UseDirectiveElementSyntax(SyntaxTree tree, TextSpan span, SimpleNameSyntax name, Token asKeyword, IdentifierNameSyntax? alias) : base(tree, span)
	{
		Name = name;
		AsKeyword = asKeyword;
		Alias = alias;

		SetParent(name);
		SetParentIfNotNull(alias);
	}

	public override string ToString()
	{
		if(Alias is null)
		{
			return $"{Name}";
		}

		return $"{Name} {AsKeyword} {Alias}";
	}
}
