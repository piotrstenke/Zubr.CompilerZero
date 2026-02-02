using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Syntax;

public sealed class AttributeSyntax : SyntaxNode
{
	public override SyntaxKind Kind => SyntaxKind.Attribute;

	public Token OpenBracketToken { get; }

	public AttributeTargetSyntax? Target { get; }

	public NameSyntax Name { get; }

	public AttributeArgumentListSyntax? ArgumentList { get; }

	public Token CloseBracketToken { get; }

	internal AttributeSyntax(
		Token openBracketToken,
		AttributeTargetSyntax? target,
		NameSyntax name,
		AttributeArgumentListSyntax? argumentList,
		Token closeBracketToken
	)
	{
		OpenBracketToken = openBracketToken;
		Target = target;
		Name = name;
		ArgumentList = argumentList;
		CloseBracketToken = closeBracketToken;

		SetParentIfNotNull(target);
		SetParent(name);
		SetParentIfNotNull(argumentList);
	}

	public override string ToString()
	{
		return $"{OpenBracketToken}{(Target is null ? "" : $"{Target} ")}{Name}{ArgumentList}{CloseBracketToken}";
	}
}
