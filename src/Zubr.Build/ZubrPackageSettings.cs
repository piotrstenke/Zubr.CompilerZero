using Tomlyn;
using Zubr.Compiler;

namespace Zubr.Build;

public sealed class ZubrPackageSettings
{
	[TomlPropertyName("target")]
	public string? Target { get; set; }

	[TomlPropertyName("lang_version")]
	public LanguageVersion LanguageVersion { get; set; }

	[TomlPropertyName("assembly_name")]
	public string? AssemblyName { get; set; }

	[TomlPropertyName("output")]
	public string? OutputPath { get; set; }

	[TomlPropertyName("output_type")]
	public OutputKind OutputKind { get; set; }
}
