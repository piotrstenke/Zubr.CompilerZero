using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class AccessorDeclarationSyntax : SyntaxNode
{
	public override SyntaxKind Kind { get; }

	public SyntaxList<AttributeSyntax> Attributes { get; }

	public TokenList Modifiers { get; }

	public Token Keyword { get; }

	public BlockSyntax? Block { get; }

	public ArrowExpressionClauseSyntax? ExpressionBody { get; }

	public Token SemicolonToken { get; }

	internal AccessorDeclarationSyntax(
		SyntaxTree tree,
		TextSpan span,
		SyntaxKind kind,
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		Token keyword,
		BlockSyntax? block,
		ArrowExpressionClauseSyntax? expressionBody,
		Token semicolonToken
	) : base(tree, span)
	{
		Kind = kind;
		Attributes = attributes;
		Modifiers = modifiers;
		Keyword = keyword;
		Block = block;
		ExpressionBody = expressionBody;
		SemicolonToken = semicolonToken;

		SetParent(attributes);
		SetParentIfNotNull(block);
		SetParentIfNotNull(expressionBody);
	}

	public override string ToString()
	{
		if(Block is not null)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword} {Block}";
		}

		if(ExpressionBody is not null)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword} {ExpressionBody}{SemicolonToken}";
		}

		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword}{SemicolonToken}";
	}
}
