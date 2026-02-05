using System;
using System.Collections.Generic;
using System.IO;
using Tomlyn;
using Tomlyn.Syntax;
using Zubr.Build.Logging;

namespace Zubr.Build;

partial class ZubrWorkspace
{
	public static ZubrWorkspace Load(string path, out ErrorMessage[]? errors)
	{
		return Load(path, Logging.Logger.Null, out errors);
	}

	public static ZubrWorkspace Load(string path, ILogger logger, out ErrorMessage[]? errors)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(path);
		ArgumentNullException.ThrowIfNull(logger);

		if (!Path.Exists(path))
		{
			throw new WorkspaceException($"Zubr package not found at path: {path}");
		}

		FileAttributes attr;

		try
		{
			attr = File.GetAttributes(path);
		}
		catch (Exception ex)
		{
			throw new WorkspaceException($"Loading package manifest file failed with error: {ex.Message} at path: {path}", ex);
		}

		ZubrManifest manifest;
		string packageFilePath;

		if (attr != default && attr.HasFlag(FileAttributes.Directory))
		{
			manifest = LoadManifestFromDirectory(path, out packageFilePath, out errors);
		}
		else
		{
			manifest = LoadManifestFromFile(path, out errors);
			packageFilePath = path;
		}

		string name = string.IsNullOrWhiteSpace(manifest.Package?.Name)
			? Path.GetFileNameWithoutExtension(packageFilePath)
			: manifest.Package.Name;

		string rootPath = Path.GetDirectoryName(packageFilePath)!;

		string? target = manifest.Settings?.Target;

		if(string.IsNullOrWhiteSpace(target))
		{
			throw new WorkspaceException("Target runtime must be specified");
		}

		if(!ZubrRuntime.TryParse(target, out ZubrRuntime runtime))
		{
			throw new WorkspaceException($"Unknown runtime: '{target}'");
		}

		return new(name, rootPath, packageFilePath, manifest, runtime, logger);
	}

	private static ZubrManifest LoadManifestFromDirectory(string path, out string packageFilePath, out ErrorMessage[]? errors)
	{
		string[] files = Directory.GetFiles(path, "*.toml");

		if(files.Length == 0)
		{
			throw new WorkspaceException($"Package manifest file not found at path: {path}");
		}

		if(files.Length > 1)
		{
			throw new WorkspaceException($"Multiple package manifest files found at path: {path}");
		}

		string targetPath = files[0];
		packageFilePath = targetPath;
		return LoadManifestFromFile(targetPath, out errors);
	}

	private static ZubrManifest LoadManifestFromFile(string path, out ErrorMessage[]? errors)
	{
		string content;

		try
		{
			content = File.ReadAllText(path);
		}
		catch(Exception ex)
		{
			throw new WorkspaceException($"Loading package manifest file failed with error: {ex.Message} at path: {path}", ex);
		}

		ZubrManifest? manifest = TryReadManifestContent(content, path, out errors);

		if(manifest is null)
		{
			return new();
		}

		return manifest;
	}

	private static ZubrManifest? TryReadManifestContent(string content, string path, out ErrorMessage[]? errors)
	{
		try
		{
			if (Toml.TryToModel(content, out ZubrManifest? manifest, out DiagnosticsBag? diagnostics, path))
			{
				errors = null;
				return manifest;
			}

			List<ErrorMessage> list = new(diagnostics.Count);

			foreach (DiagnosticMessage diag in diagnostics)
			{
				list.Add(new()
				{
					Message = diag.Message,
					Level = diag.Kind == DiagnosticMessageKind.Warning
						? ErrorLevel.Warning
						: ErrorLevel.Error,
					Line = diag.Span.Start.Line + 1,
					Column = diag.Span.Start.Column + 1,
				});
			}

			errors = list.ToArray();

			return null;
		}
		catch (TomlException ex)
		{
			throw new WorkspaceException($"Loading package manifest file failed with error: {ex.Message} at path: {path}", ex);
		}
	}
}
