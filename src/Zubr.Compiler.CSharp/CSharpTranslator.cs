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
		CompilationUnitSyntax root = Translate((Syntax.CompilationUnitSyntax)syntaxTree.Root);

		return (CSharpSyntaxTree)CSharpSyntaxTree.Create(
			root,
			encoding: syntaxTree.Encoding,
			path: syntaxTree.SourcePath,
			options: new CSharpParseOptions(languageVersion: Options.LanguageVersion)
		);
	}
}
