using ConsoleAppFramework;
using System;
using System.IO;
using Zubr.Build;
using Zubr.Build.Logging;
using Zubr.Compiler;
using Zubr.Compiler.Diagnostics;
using Zubr.Compiler.Emit;

ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

args = ["build", "-p", "C:\\Users\\promant\\Desktop\\code-projects\\Zubr\\zubrlib\\src\\core"];

app.Add("build", Commands.Build);

await app.RunAsync(args);

static class Commands
{
	/// <summary>
	/// Builds the package.
	/// </summary>
	/// <param name="path">-p, Path to the package root directory or the package manifest file.</param>
	public static void Build(string? path = null)
	{
		string targetPath = string.IsNullOrWhiteSpace(path)
			? Environment.CurrentDirectory
			: Path.GetFullPath(path);

		ZubrWorkspace workspace = ZubrWorkspace.Load(targetPath, Logger.Console(LogLevel.Info), out ErrorMessage[]? errors);

		Console.WriteLine("Building zubr package...");
		Console.WriteLine();

		if(errors is not null)
		{
			foreach (ErrorMessage error in errors)
			{
				Console.WriteLine($"({error.Line}, {error.Column}): [{error.Level}] {error.Message}");
			}
		}

		Compilation compilation = workspace.CreateCompilation();

		if(compilation.HasDiagnostics)
		{
			WriteDiagnostics(compilation.GetDiagnostics());
		}

		IEmitter emitter = workspace.CreateEmitter();

		byte[]? bytes = emitter.Emit(compilation, out DiagnosticMessage[]? diagnostics);

		if(diagnostics is not null)
		{
			WriteDiagnostics(diagnostics);
		}

		if(bytes is null)
		{
			Console.WriteLine("Build failed. No output generated.");
			return;
		}

		string outputPath = Path.Combine(workspace.OutputPath, compilation.AssemblyName);

		try
		{
			File.WriteAllBytes(outputPath, bytes);
		}
		catch(Exception ex)
		{
			Console.WriteLine($"Failed to write compilation result to file at path '{outputPath}' with error {ex.Message}");
		}
	}

	private static void WriteDiagnostics(DiagnosticMessage[] diagnostics)
	{
		foreach (DiagnosticMessage diagnostic in diagnostics)
		{
			Console.WriteLine($"({diagnostic.Position}): [{diagnostic.Severity}] {diagnostic.Message} at file '{diagnostic.SourceFile}'");
		}
	}
}
