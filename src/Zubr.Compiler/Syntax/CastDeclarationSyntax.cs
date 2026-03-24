using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class CastDeclarationSyntax : BaseFunctionDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.CastDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public Token CastKeyword { get; }

	public TypeSyntax Type { get; }

	public override ParameterListSyntax ParameterList { get; }

	public override BlockSyntax? Body { get; }

	public override ArrowExpressionClauseSyntax? ExpressionBody { get; }

	public override Token SemicolonToken { get; }

	internal CastDeclarationSyntax(
		SyntaxTree tree,
		TextSpan span,
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		Token castKeyword,
		TypeSyntax type,
		ParameterListSyntax parameterList,
		BlockSyntax? body,
		ArrowExpressionClauseSyntax? expressionBody,
		Token semicolonToken
	) : base(tree, span)
	{
		Attributes = attributes;
		Modifiers = modifiers;
		CastKeyword = castKeyword;
		Type = type;
		ParameterList = parameterList;
		Body = body;
		ExpressionBody = expressionBody;
		SemicolonToken = semicolonToken;

		SetParent(attributes);
		SetParent(type);
		SetParent(parameterList);
		SetParentIfNotNull(body);
		SetParentIfNotNull(expressionBody);
	}

	public override string ToString()
	{
		if (Body is not null)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{CastKeyword} {Type}{ParameterList} {Body}";
		}

		if (ExpressionBody is not null)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{CastKeyword} {Type}{ParameterList} {ExpressionBody}{SemicolonToken}";
		}

		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{CastKeyword} {Type}{ParameterList}{SemicolonToken}";
	}
}
