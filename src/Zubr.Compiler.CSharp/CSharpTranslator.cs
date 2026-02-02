using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
		CompilationUnitSyntax root = Translate(syntaxTree.Root);

		return (CSharpSyntaxTree)CSharpSyntaxTree.Create(
			root,
			encoding: syntaxTree.Encoding,
			options: new CSharpParseOptions(languageVersion: Options.LanguageVersion)
		);
	}
}
