using System;
using System.Diagnostics.CodeAnalysis;
using Zubr.Compiler.Diagnostics;

namespace Zubr.Compiler.Emit;

public class EmitResult
{
	private DiagnosticMessage[]? _diagnostics;

	[MemberNotNullWhen(true, nameof(Data))]
	public bool IsSuccess => Data is not null;

	public byte[]? Data { get; }

	public DiagnosticMessage[] Diagnostics => _diagnostics ??= Array.Empty<DiagnosticMessage>();

	[MemberNotNullWhen(false, nameof(Data))]
	public bool HasDiagnostics => _diagnostics is not null;

	protected internal EmitResult(byte[]? data, DiagnosticMessage[]? diagnostics)
	{
		Data = data;
		_diagnostics = diagnostics;
	}
}
