using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class FunctionDeclarationSyntax : BaseFunctionDeclarationSyntax
{
	public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;

	public override SyntaxList<AttributeSyntax> Attributes { get; }

	public override TokenList Modifiers { get; }

	public TypeSyntax ReturnType { get; }

	public Token Identifier { get; }

	public TypeParameterListSyntax? TypeParameterList { get; }

	public override ParameterListSyntax ParameterList { get; }

	public TypeParameterConstraintListSyntax? ConstraintList { get; }

	public override BlockSyntax? Body { get; }

	public override ArrowExpressionClauseSyntax? ExpressionBody { get; }

	public override Token SemicolonToken { get; }

	internal FunctionDeclarationSyntax(
		SyntaxTree tree,
		TextSpan span,
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
	) : base(tree, span)
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

		if(ExpressionBody is not null)
		{
			return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{ReturnType} {Identifier}{TypeParameterList}{ParameterList}{(ConstraintList is null ? "" : $" {ConstraintList}")} {ExpressionBody}{SemicolonToken}";
		}

		return $"{(Modifiers.Any() ? $"{Modifiers} " : "")}{ReturnType} {Identifier}{TypeParameterList}{ParameterList}{(ConstraintList is null ? "" : $" {ConstraintList}")}{SemicolonToken}";
	}
}
