using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class BaseTypeListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.BaseTypeList;

	public Token ColonToken { get; }

	public SeparatedSyntaxList<BaseTypeSyntax> Types { get; }

	internal BaseTypeListSyntax(SyntaxTree tree, TextSpan span, Token colonToken, SeparatedSyntaxList<BaseTypeSyntax> types) : base(tree, span)
	{
		ColonToken = colonToken;
		Types = types;

		SetParent(types);
	}

	public override string ToString()
	{
		return $"{ColonToken} {Types}";
	}
}
