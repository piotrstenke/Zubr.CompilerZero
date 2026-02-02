using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class LocalFunctionStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.LocalFunctionStatement;

	public SyntaxList<AttributeSyntax> Attributes { get; }

	public TokenList Modifiers { get; }

	public TypeSyntax ReturnType { get; }

	public Token Identifier { get; }

	public TypeParameterListSyntax? TypeParameterList { get; }

	public ParameterListSyntax ParameterList { get; }

	public TypeParameterConstraintListSyntax? ConstraintList { get; }

	public BlockSyntax? Body { get; }

	public ArrowExpressionClauseSyntax? ExpressionBody { get; }

	public Token SemicolonToken { get; }

	internal LocalFunctionStatementSyntax(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		TypeSyntax returnType,
		Token identifier,
		TypeParameterListSyntax? typeParameterList,
		ParameterListSyntax parameterList,
		TypeParameterConstraintListSyntax? constraintList,
		BlockSyntax? body,
		ArrowExpressionClauseSyntax? expressionBody,
		Token semicolonToken
	)
	{
		Attributes = attributes;
		Modifiers = modifiers;
		ReturnType = returnType;
		Identifier = identifier;
		ParameterList = parameterList;
		TypeParameterList = typeParameterList;
		ConstraintList = constraintList;
		Body = body;
		ExpressionBody = expressionBody;
		SemicolonToken = semicolonToken;

		SetParent(attributes);
		SetParent(returnType);
		SetParent(parameterList);
		SetParentIfNotNull(typeParameterList);
		SetParentIfNotNull(constraintList);
		SetParentIfNotNull(body);
		SetParentIfNotNull(expressionBody);
	}

	public override string ToString()
	{
		if (Body is not null)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{ReturnType} {Identifier}{TypeParameterList}{ParameterList}{(ConstraintList is null ? "" : $" {ConstraintList}")} {Body}";
		}

		if (ExpressionBody is not null)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{ReturnType} {Identifier}{TypeParameterList}{ParameterList}{(ConstraintList is null ? "" : $" {ConstraintList}")} {ExpressionBody}{SemicolonToken}";
		}

		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{ReturnType} {Identifier}{TypeParameterList}{ParameterList}{(ConstraintList is null ? "" : $" {ConstraintList}")}{SemicolonToken}";
	}
}
