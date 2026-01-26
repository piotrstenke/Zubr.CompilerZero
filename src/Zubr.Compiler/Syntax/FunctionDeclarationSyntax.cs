using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class FunctionDeclarationSyntax : MemberDeclarationSyntax
{
	public override SyntaxKind Kind =>  SyntaxKind.FunctionDeclaration;

	public override SyntaxTokenList Modifiers { get; }

	public TypeSyntax ReturnType { get; }

	public SyntaxToken Identifier { get; }

	public TypeParameterListSyntax? TypeParameterList { get; }

	public ParameterListSyntax ParameterList { get; }

	public TypeParameterConstraintListSyntax? ConstraintList { get; }

	public BlockSyntax Body { get; }

	internal FunctionDeclarationSyntax(SyntaxTokenList modifiers, TypeSyntax returnType, SyntaxToken identifier, TypeParameterListSyntax? typeParameterList, ParameterListSyntax parameterList, TypeParameterConstraintListSyntax? constraintList, BlockSyntax body)
	{
		Modifiers = modifiers;
		ReturnType = returnType;
		Identifier = identifier;
		ParameterList = parameterList;
		TypeParameterList = typeParameterList;
		ConstraintList = constraintList;
		Body = body;

		SetParent(returnType);
		SetParent(parameterList);
		SetParentIfNotNull(typeParameterList);
		SetParentIfNotNull(constraintList);
		SetParent(body);
	}

	public override string ToString()
	{
		return $"{Modifiers} {ReturnType} {Identifier}{TypeParameterList}{ParameterList}{(ConstraintList is null ? "" : $" {ConstraintList}")} {Body}";
	}
}
