using System.Collections.Generic;
using Tomlyn;

namespace Zubr.Build;

public sealed class ZubrManifest
{
	[TomlPropertyName("pack")]
	public ZubrPackage? Package { get; set; }

	[TomlPropertyName("set")]
	public ZubrPackageSettings? Settings { get; set; }

	[TomlPropertyName("deps")]
	public Dictionary<string, string>? Dependencies { get; set; }

	[TomlPropertyName("tasks")]
	public Dictionary<string, string>? Tasks { get; set; }
}
