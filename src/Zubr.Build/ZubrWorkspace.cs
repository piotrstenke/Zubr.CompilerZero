using System;
using System.Collections.Generic;
using System.IO;
using Zubr.Build.Logging;
using Zubr.Compiler;
using Zubr.Compiler.CSharp;
using Zubr.Compiler.Emit;

namespace Zubr.Build;

public sealed partial class ZubrWorkspace
{
	private readonly ZubrManifest _manifest;

	public string Name { get; }

	public string RootPath { get; }

	public string PackageFilePath { get; }

	public string OutputPath { get; }

	public ILogger Logger { get; }

	public ZubrPackage Package { get; }

	public ZubrRuntime Runtime { get; }

	internal ZubrWorkspace(
		string name,
		string rootPath,
		string packageFilePath,
		ZubrManifest manifest,
		ZubrRuntime runtime,
		ILogger logger
	)
	{
		_manifest = manifest;

		Name = name;
		RootPath = rootPath;
		PackageFilePath = packageFilePath;
		Logger = logger;
		OutputPath = string.IsNullOrWhiteSpace(manifest.Settings?.OutputPath)
			? Path.Combine(rootPath, "out")
			: manifest.Settings.OutputPath;

		Package = manifest.Package ?? new();
		Runtime = runtime;
	}

	public IEnumerable<string> GetFiles()
	{
		return Directory.EnumerateFiles(RootPath, "*.zr", SearchOption.AllDirectories);
	}

	public Compilation CreateCompilation()
	{
		List<SyntaxTree> syntaxTrees = GetSyntaxTrees();

		Compilation compilation = Compilation.Create(
			assemblyName: Name + ".dll",
			languageVersion: GetLanguageVersion(),
			outputKind: _manifest.Settings?.OutputKind ?? default,
			syntaxTrees: syntaxTrees
		);

		return compilation;
	}

	public IEmitter CreateEmitter()
	{
		if(Runtime.IsDotNet())
		{
			return new CSharpEmitter();
		}

		throw new InvalidOperationException("Only dotnet runtime is supported");
	}

	private LanguageVersion GetLanguageVersion()
	{
		if(_manifest.Settings?.LanguageVersion is null || _manifest.Settings.LanguageVersion == LanguageVersion.Default)
		{
			return LanguageVersion.Alpha;
		}

		return _manifest.Settings.LanguageVersion;
	}

	private List<SyntaxTree> GetSyntaxTrees()
	{
		List<SyntaxTree> syntaxTrees = new();

		foreach (string file in GetFiles())
		{
			string content;

			try
			{
				content = File.ReadAllText(file);
			}
			catch (Exception ex)
			{
				Logger.LogError($"Could not read contents of file '{file}': {ex.Message}");
				continue;
			}

			Logger.LogInfo($"Parsing syntax tree at path: '{file}'");
			SyntaxTree tree = SyntaxTree.Parse(SourceText.FromSource(content, file));
			syntaxTrees.Add(tree);
		}

		return syntaxTrees;
	}
}
