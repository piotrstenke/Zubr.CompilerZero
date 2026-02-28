using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class LabelStatementSyntax : StatementSyntax
{
	public override SyntaxKind Kind => SyntaxKind.LabelStatement;

	public Token Identifier { get; }

	public Token ColonToken { get; }

	public StatementSyntax Statement { get; }

	internal LabelStatementSyntax(SyntaxTree tree, TextSpan span, Token identifier, Token colonToken, StatementSyntax statement) : base(tree, span)
	{
		Identifier = identifier;
		ColonToken = colonToken;
		Statement = statement;

		SetParent(statement);
	}
}
