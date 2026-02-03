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
	public string Name { get; }

	public string RootPath { get; }

	public string PackageFilePath { get; }

	public string OutputPath { get; }

	public ZubrManifest Manifest { get; }

	public ILogger Logger { get; }

	internal ZubrWorkspace(
		string name,
		string rootPath,
		string packageFilePath,
		ZubrManifest manifest,
		ILogger logger
	)
	{
		Name = name;
		RootPath = rootPath;
		PackageFilePath = packageFilePath;
		Manifest = manifest;
		Logger = logger;
		OutputPath = string.IsNullOrWhiteSpace(manifest.Settings?.OutputPath)
			? Path.Combine(rootPath, "out")
			: manifest.Settings.OutputPath;
	}

	public IEnumerable<string> GetFiles()
	{
		return Directory.EnumerateFiles(RootPath, "*.zr", SearchOption.AllDirectories);
	}

	public Compilation CreateCompilation()
	{
		List<SyntaxTree> syntaxTrees = GetSyntaxTrees();

		Compilation compilation = Compilation.Create(
			assemblyName: Name,
			languageVersion: GetLanguageVersion(),
			outputKind: Manifest.Settings?.OutputKind ?? default,
			syntaxTrees: syntaxTrees
		);

		return compilation;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
	public IEmitter CreateEmitter()
	{
		CSharpEmitter emitter = new();
		return emitter;
	}

	private LanguageVersion GetLanguageVersion()
	{
		if(Manifest.Settings?.LanguageVersion is null || Manifest.Settings.LanguageVersion == LanguageVersion.Default)
		{
			return LanguageVersion.Alpha;
		}

		return Manifest.Settings.LanguageVersion;
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
