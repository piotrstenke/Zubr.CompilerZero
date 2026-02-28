using ConsoleAppFramework;
using System;
using System.IO;
using Zubr.Build;
using Zubr.Build.Logging;
using Zubr.Compiler;
using Zubr.Compiler.CSharp;
using Zubr.Compiler.Diagnostics;
using Zubr.Compiler.Emit;

ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

args = ["build", "-p", "C:\\Users\\promant\\Desktop\\code-projects\\Zubr\\zubrlib\\src\\core", "-t"];

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
	public static void Build(string? path = null, bool showTrees = false)
	{
		string targetPath = string.IsNullOrWhiteSpace(path)
			? Environment.CurrentDirectory
			: Path.GetFullPath(path);

		ZubrWorkspace workspace = ZubrWorkspace.Load(targetPath, Logger.Console(LogLevel.Info), out ErrorMessage[]? errors);

		Console.WriteLine("Building zubr package...");
		Console.WriteLine();

		if (errors is not null)
		{
			foreach (ErrorMessage error in errors)
			{
				Console.WriteLine($"[{error.Level}]: {error.Message} at ({error.Line}, {error.Column})");
			}
		}

		Compilation compilation = workspace.CreateCompilation();

		if (compilation.HasDiagnostics)
		{
			WriteDiagnostics(compilation.GetDiagnostics());
		}

		IEmitter emitter = workspace.CreateEmitter();

		EmitResult result = emitter.Emit(compilation);

		if (result.HasDiagnostics)
		{
			WriteDiagnostics(result.Diagnostics);
		}

		if (!result.IsSuccess)
		{
			Console.WriteLine();
			Console.WriteLine("Build failed. No output generated.");

			OutputCSharpSyntaxTrees(showTrees, result);
			return;
		}

		OutputCSharpSyntaxTrees(showTrees, result);

		string outputPath = Path.Combine(workspace.OutputPath, compilation.AssemblyName);

		try
		{
			File.WriteAllBytes(outputPath, result.Data);
		}
		catch (Exception ex)
		{
			Console.WriteLine();
			Console.WriteLine($"Failed to write compilation result to file at path '{outputPath}' with error {ex.Message}");
		}

		Console.WriteLine();
		Console.WriteLine($"Data written to file '{outputPath}'");
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
			Console.WriteLine($"[{diagnostic.Severity}]: {diagnostic.Message} at {diagnostic.Location}");
		}
	}
}
