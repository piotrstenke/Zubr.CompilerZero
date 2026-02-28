using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class AccessorListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.AccessorList;

	public Token OpenBraceToken { get; }

	public SyntaxList<AccessorDeclarationSyntax> Accessors { get; }

	public Token CloseBraceToken { get; }

	internal AccessorListSyntax(SyntaxTree tree, TextSpan span, Token openBraceToken, SyntaxList<AccessorDeclarationSyntax> accessors, Token closeBraceToken) : base(tree, span)
	{
		OpenBraceToken = openBraceToken;
		Accessors = accessors;
		CloseBraceToken = closeBraceToken;

		SetParent(accessors);
	}

	public override string ToString()
	{
		return $"{OpenBraceToken} {Accessors} {CloseBraceToken}";
	}
}
