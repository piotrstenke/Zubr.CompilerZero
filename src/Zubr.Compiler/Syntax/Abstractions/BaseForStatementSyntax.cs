using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class BaseForStatementSyntax : StatementSyntax
{
	public abstract Token ForKeyword { get; }

	public abstract Token OpenParenToken { get; }

	public abstract Token CloseParenToken { get; }

	public abstract StatementSyntax Statement { get; }

	internal BaseForStatementSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{
	}
}
