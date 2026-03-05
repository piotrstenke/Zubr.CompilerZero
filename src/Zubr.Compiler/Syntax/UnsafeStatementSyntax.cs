using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class UnsafeStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.UnsafeStatement;

	public Token UnsafeKeyword { get; }

	public BlockSyntax Block { get; }

	internal UnsafeStatementSyntax(SyntaxTree tree, TextSpan span, Token unsafeKeyword, BlockSyntax block) : base(tree, span)
	{
		UnsafeKeyword = unsafeKeyword;
		Block = block;

		SetParent(block);
	}
}
