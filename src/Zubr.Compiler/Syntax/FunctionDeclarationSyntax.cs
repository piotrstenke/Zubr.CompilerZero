using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class FunctionDeclarationSyntax : MemberDeclarationSyntax
{
	public override SyntaxKind Kind =>  SyntaxKind.FunctionDeclaration;

	public override SyntaxTokenList Modifiers { get; }

	public TypeSyntax ReturnType { get; }

	public SyntaxToken Identifier { get; }

	public ParameterListSyntax ParameterList { get; }

	public BlockSyntax Body { get; }

	internal FunctionDeclarationSyntax(SyntaxTokenList modifiers, TypeSyntax returnType, SyntaxToken identifier, ParameterListSyntax parameterList, BlockSyntax body)
	{
		Modifiers = modifiers;
		ReturnType = returnType;
		Identifier = identifier;
		ParameterList = parameterList;
		Body = body;

		SetParent(returnType);
		SetParent(parameterList);
		SetParent(body);
	}
}
