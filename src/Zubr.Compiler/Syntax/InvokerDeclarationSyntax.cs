using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class InvokerDeclarationSyntax : BaseFunctionDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.InvokerDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public TypeSyntax ReturnType { get; }

	public Token SelfKeyword { get; }

	public override ParameterListSyntax ParameterList { get; }

	public override BlockSyntax? Body { get; }

	public override ArrowExpressionClauseSyntax? ExpressionBody { get; }

	public override Token SemicolonToken { get; }

	internal InvokerDeclarationSyntax(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		TypeSyntax returnType,
		Token selfKeyword,
		ParameterListSyntax parameterList,
		BlockSyntax? body,
		ArrowExpressionClauseSyntax? expressionBody,
		Token semicolonToken
	)
	{
		Attributes = attributes;
		Modifiers = modifiers;
		ReturnType = returnType;
		SelfKeyword = selfKeyword;
		ParameterList = parameterList;
		Body = body;
		ExpressionBody = expressionBody;
		SemicolonToken = semicolonToken;

		SetParent(attributes);
		SetParent(returnType);
		SetParent(parameterList);
		SetParentIfNotNull(body);
		SetParentIfNotNull(expressionBody);
	}

	public override string ToString()
	{
		if (Body is not null)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{ReturnType} {SelfKeyword}{ParameterList} {Body}";
		}

		if (ExpressionBody is not null)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{ReturnType} {SelfKeyword}{ParameterList} {ExpressionBody}{SemicolonToken}";
		}

		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{ReturnType} {SelfKeyword}{ParameterList}{SemicolonToken}";
	}
}
