using Microsoft.CodeAnalysis.CSharp;

namespace Zubr.Compiler.CSharp;

internal sealed partial class CSharpTranslator
{
	public CSharpTranslatorOptions Options { get; }

	internal CSharpTranslator(CSharpTranslatorOptions options)
	{
		Options = options;
	}

	public static CSharpTranslator Create()
	{
		return Create(new());
	}

	public static CSharpTranslator Create(CSharpTranslatorOptions options)
	{
		return new(options);
	}

	public CSharpSyntaxTree Translate(SyntaxTree syntaxTree)
	{
		return (CSharpSyntaxTree)CSharpSyntaxTree.Create(Translate(syntaxTree.Root), encoding: syntaxTree.Encoding);
	}

	private static Microsoft.CodeAnalysis.SyntaxToken Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind kind)
	{
		return SyntaxFactory.Token(kind);
	}
}
