using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class ElseClauseSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.ElseClause;

	public Token ElseKeyword { get; }

	public StatementSyntax Statement { get; }

	internal ElseClauseSyntax(SyntaxTree tree, TextSpan span, Token elseKeyword, StatementSyntax statement) : base(tree, span)
	{
		ElseKeyword = elseKeyword;
		Statement = statement;

		SetParent(statement);
	}

	public override string ToString()
	{
		return $"{ElseKeyword} {Statement}";
	}
}
