using Zubr.Compiler.Diagnostics;

namespace Zubr.Compiler.Emit;

public interface IEmitter
{
	byte[]? Emit(Compilation compilation, out DiagnosticMessage[]? diagnostics);
}
