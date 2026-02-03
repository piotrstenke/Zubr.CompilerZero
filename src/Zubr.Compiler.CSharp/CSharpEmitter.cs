using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Zubr.Compiler.Diagnostics;
using Zubr.Compiler.Emit;

using EmitResult = Zubr.Compiler.Emit.EmitResult;
using MEmitResult = Microsoft.CodeAnalysis.Emit.EmitResult;

namespace Zubr.Compiler.CSharp;

internal sealed class CSharpEmitter : IEmitter
{
	public EmitResult Emit(Compilation compilation)
	{
		CSharpCompilation csharpCompilation = CreateCSharpCompilation(compilation);

		using MemoryStream stream = new();

		MEmitResult result = csharpCompilation.Emit(stream);

		DiagnosticMessage[]? diagnostics = GetDiagnostics(result);

		if(!result.Success)
		{
			return new CSharpEmitResult(null, csharpCompilation, diagnostics);
		}

		byte[] data = stream.ToArray();
		return new CSharpEmitResult(data, csharpCompilation, diagnostics);
	}

	private static CSharpCompilation CreateCSharpCompilation(Compilation compilation)
	{
		Microsoft.CodeAnalysis.OutputKind kind = GetOutputKind(compilation);

		CSharpTranslator translator = CSharpTranslator.Create();

		Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees = compilation.SyntaxTrees
			.Select(translator.Translate)
			.ToArray();

		CSharpCompilation csharpCompilation = RoslynUtilities.CreateCompilation(
			compilation.AssemblyName,
			kind,
			syntaxTrees
		);

		return csharpCompilation;
	}

	private static DiagnosticMessage[]? GetDiagnostics(MEmitResult result)
	{
		ImmutableArray<Diagnostic> diagnostics = result.Diagnostics;

		if (diagnostics.IsDefaultOrEmpty)
		{
			return null;
		}

		DiagnosticMessage[] array = new DiagnosticMessage[diagnostics.Length];

		for (int i = 0; i < array.Length; i++)
		{
			Diagnostic diagnostic = diagnostics[i];

			array[i] = new(
				diagnostic.Descriptor.Id,
				diagnostic.Location.SourceSpan.Start,
				diagnostic.GetMessage(),
				GetSeverity(diagnostic.Severity),
				diagnostic.Location.SourceTree?.FilePath
			);
		}

		return array;
	}

	private static Diagnostics.DiagnosticSeverity GetSeverity(Microsoft.CodeAnalysis.DiagnosticSeverity value)
	{
		return (Diagnostics.DiagnosticSeverity)value;
	}

	private static Microsoft.CodeAnalysis.OutputKind GetOutputKind(Compilation compilation)
	{
		return compilation.OutputKind switch
		{
			OutputKind.Console => Microsoft.CodeAnalysis.OutputKind.ConsoleApplication,
			OutputKind.App => Microsoft.CodeAnalysis.OutputKind.WindowsApplication,
			OutputKind.Lib => Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary,
			_ => throw new UnreachableException()
		};
	}
}
