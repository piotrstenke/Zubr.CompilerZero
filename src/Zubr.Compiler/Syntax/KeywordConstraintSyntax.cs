using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax;

public sealed class KeywordConstraintSyntax : TypeParameterConstraintSyntax
{
	public override SyntaxKind Kind { get; }

	public Token Keyword { get; }

	public Token QuestionToken { get; }

	internal KeywordConstraintSyntax(
		SyntaxTree tree,
		TextSpan span,
		SyntaxKind kind,
		Token keyword,
		Token questionToken
	) : base(tree, span)
	{
		Kind = kind;
		Keyword = keyword;
		QuestionToken = questionToken;
	}

	public override string ToString()
	{
		return $"{Keyword}";
	}
}
