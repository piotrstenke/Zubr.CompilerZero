using ConsoleAppFramework;
using System;
using System.Diagnostics;
using System.IO;
using Zubr.Build;
using Zubr.Build.Logging;
using Zubr.Compiler;
using Zubr.Compiler.CSharp;
using Zubr.Compiler.Diagnostics;
using Zubr.Compiler.Emit;

ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

args = ["build", "-p", "..\\..\\..\\..\\..\\zubrlib\\src\\core", "-t", "-d"];

app.Add("build", Commands.Build);

await app.RunAsync(args);

Console.ReadKey();

static class Commands
{
	/// <summary>
	/// Builds the package.
	/// </summary>
	/// <param name="path">-p, Path to the package root directory or the package manifest file.</param>
	/// <param name="showTrees">-t, Show generated C# syntax trees.</param>
	/// <param name="debug">-d, Show debug and trace logs.</param>
	public static void Build(string? path = null, bool showTrees = false, bool debug = false)
	{
		string targetPath = string.IsNullOrWhiteSpace(path)
			? Environment.CurrentDirectory
			: Path.GetFullPath(path);

		ZubrWorkspace workspace = ZubrWorkspace.Load(targetPath, Logger.Console(debug ? LogLevel.Trace : LogLevel.Info), out ErrorMessage[]? errors);

		Console.WriteLine("Building zubr package...");
		Console.WriteLine();

		if (errors is not null)
		{
			foreach (ErrorMessage error in errors)
			{
				Console.WriteLine($"[{error.Level}]: {error.Message} at ({error.Line}, {error.Column})");
			}
		}

		Stopwatch watch = Stopwatch.StartNew();

		Compilation compilation = workspace.CreateCompilation();

		watch.Stop();

		if (compilation.HasDiagnostics)
		{
			Console.WriteLine();
			Console.WriteLine("Build diagnostics:");

			WriteDiagnostics(compilation.GetDiagnostics());
		}

		Console.WriteLine();
		Console.WriteLine($"Build took {watch.ElapsedMilliseconds}ms");

		watch.Restart();

		IEmitter emitter = workspace.CreateEmitter();

		EmitResult result = emitter.Emit(compilation);

		watch.Stop();

		if (result.HasDiagnostics)
		{
			Console.WriteLine();
			Console.WriteLine("Emit diagnostics:");

			WriteDiagnostics(result.Diagnostics);
		}

		Console.WriteLine();
		Console.WriteLine($"Emit took {watch.ElapsedMilliseconds}ms");

		if (!result.IsSuccess)
		{
			Console.WriteLine();
			Console.WriteLine("Build failed. No output generated.");

			OutputCSharpSyntaxTrees(showTrees, result);
			return;
		}

		string outputPath = Path.Combine(workspace.OutputPath, compilation.AssemblyName);

		try
		{
			Directory.CreateDirectory(workspace.OutputPath);
			File.WriteAllBytes(outputPath, result.Data);
		}
		catch (Exception ex)
		{
			Console.WriteLine();
			Console.WriteLine($"Failed to write compilation result to file at path '{outputPath}' with error: {ex.Message}");
		}

		Console.WriteLine();
		Console.WriteLine($"Data written to file '{outputPath}'");

		OutputCSharpSyntaxTrees(showTrees, result);
	}

	private static void OutputCSharpSyntaxTrees(bool showTrees, EmitResult result)
	{
		if (showTrees && result is CSharpEmitResult csharp)
		{
			Console.WriteLine();
			Console.WriteLine("Generated C# syntax trees:");

			foreach (Microsoft.CodeAnalysis.SyntaxTree tree in csharp.Compilation.SyntaxTrees)
			{
				Console.WriteLine();
				Console.WriteLine("---------------------------------------------------------------------------------------------------------");
				Console.WriteLine(tree.FilePath);
				Console.WriteLine("---------------------------------------------------------------------------------------------------------");
				Console.WriteLine(tree.ToString());
				Console.WriteLine("---------------------------------------------------------------------------------------------------------");
			}
		}
	}

	private static void WriteDiagnostics(DiagnosticMessage[] diagnostics)
	{
		Console.WriteLine();

		foreach (DiagnosticMessage diagnostic in diagnostics)
		{
			if(diagnostic.Location.IsValid)
			{
				Console.WriteLine($"[{diagnostic.Severity}]: {diagnostic.Message} at {diagnostic.Location}");
			}
			else
			{
				Console.WriteLine($"[{diagnostic.Severity}]: {diagnostic.Message}");
			}
		}
	}
}
