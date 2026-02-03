using Microsoft.CodeAnalysis.CSharp;
using Zubr.Compiler.Diagnostics;

using EmitResult = Zubr.Compiler.Emit.EmitResult;

namespace Zubr.Compiler.CSharp;

public sealed class CSharpEmitResult : EmitResult
{
	public CSharpCompilation Compilation { get; }

	internal CSharpEmitResult(byte[]? data, CSharpCompilation compilation, DiagnosticMessage[]? diagnostics) : base(data, diagnostics)
	{
		Compilation = compilation;
	}
}
