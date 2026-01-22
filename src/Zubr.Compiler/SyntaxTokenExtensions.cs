namespace Zubr.Compiler;

public static class SyntaxTokenExtensions
{
	public static bool IsKind(this in SyntaxToken token, SyntaxKind kind)
	{
		return token.Kind == kind; 
	}
}
