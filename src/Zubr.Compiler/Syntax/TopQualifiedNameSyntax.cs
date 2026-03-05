using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class TopQualifiedNameSyntax : NameSyntax
{
	public override SyntaxKind Kind => SyntaxKind.TopQualifiedName;

	public Token ColonColonToken { get; }

	public SimpleNameSyntax Name { get; }

	internal TopQualifiedNameSyntax(SyntaxTree tree, TextSpan span, Token colonColonToken, SimpleNameSyntax name) : base(tree, span)
	{
		ColonColonToken = colonColonToken;
		Name = name;

		SetParent(name);
	}

	public override string ToString()
	{
		return $"{ColonColonToken}{Name}";
	}
}
