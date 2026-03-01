using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

public abstract class UseDirectiveSyntax : SyntaxNode
{
	public abstract Token UseKeyword { get; }

	public abstract Token SemicolonToken { get; }

	internal UseDirectiveSyntax(SyntaxTree tree, TextSpan span) : base(tree, span)
	{
	}
}
