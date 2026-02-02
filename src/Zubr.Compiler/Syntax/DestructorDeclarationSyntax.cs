using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class DestructorDeclarationSyntax : BaseFunctionDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.DestructorDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public Token Keyword { get; }

	public override ParameterListSyntax ParameterList { get; }

	public override BlockSyntax? Body { get; }

	public override ArrowExpressionClauseSyntax? ExpressionBody { get; }

	public override Token SemicolonToken { get; }

	internal DestructorDeclarationSyntax(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		Token keyword,
		ParameterListSyntax parameterList,
		BlockSyntax? body,
		ArrowExpressionClauseSyntax? expressionBody,
		Token semicolonToken
	)
	{
		Attributes = attributes;
		Modifiers = modifiers;
		Keyword = keyword;
		ParameterList = parameterList;
		Body = body;
		ExpressionBody = expressionBody;
		SemicolonToken = semicolonToken;

		SetParent(attributes);
		SetParent(parameterList);
		SetParentIfNotNull(body);
		SetParentIfNotNull(expressionBody);
	}

	public override string ToString()
	{
		if (Body is not null)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword}{ParameterList} {Body}";
		}

		if (ExpressionBody is not null)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword}{ParameterList} {ExpressionBody}{SemicolonToken}";
		}

		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{Keyword}{ParameterList}{SemicolonToken}";
	}
}
