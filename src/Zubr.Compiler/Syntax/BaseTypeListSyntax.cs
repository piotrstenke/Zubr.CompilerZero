using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class BaseTypeListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.BaseTypeList;

	public Token ColonToken { get; }

	public SeparatedSyntaxList<BaseTypeSyntax> Types { get; }

	internal BaseTypeListSyntax(Token colonToken, SeparatedSyntaxList<BaseTypeSyntax> types)
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
