using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class GenericNameSyntax : SimpleNameSyntax
{
	public override SyntaxKind Kind => SyntaxKind.GenericName;

	public override SyntaxToken Identifier { get; }

	public TypeArgumentListSyntax TypeArgumentList { get; }

	internal GenericNameSyntax(SyntaxToken identifier, TypeArgumentListSyntax typeArgumentList)
	{
		Identifier = identifier;
		TypeArgumentList = typeArgumentList;

		SetParent(typeArgumentList);
	}

	public override string ToString()
	{
		return $"{Identifier}{TypeArgumentList}";
	}
}
