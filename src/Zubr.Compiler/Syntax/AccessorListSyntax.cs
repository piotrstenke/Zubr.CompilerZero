using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class AccessorListSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.AccessorList;

	public Token OpenBraceToken { get; }

	public SyntaxList<AccessorDeclarationSyntax> Accessors { get; }

	public Token CloseBraceToken { get; }

	internal AccessorListSyntax(Token openBraceToken, SyntaxList<AccessorDeclarationSyntax> accessors, Token closeBraceToken)
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
