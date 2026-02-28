using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class GenericNameSyntax : SimpleNameSyntax
{
	public override SyntaxKind Kind => SyntaxKind.GenericName;

	public override Token Identifier { get; }

	public TypeArgumentListSyntax TypeArgumentList { get; }

	internal GenericNameSyntax(SyntaxTree tree, TextSpan span, Token identifier, TypeArgumentListSyntax typeArgumentList) : base(tree, span)
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
