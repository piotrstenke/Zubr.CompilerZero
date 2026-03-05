using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using zubr.interop.csharp;

namespace Zubr.Compiler.CSharp;

internal static class RoslynUtilities
{
	public static CSharpCompilation CreateCompilation(
		string assemblyName,
		Microsoft.CodeAnalysis.OutputKind outputKind,
		params Microsoft.CodeAnalysis.SyntaxTree[]? syntaxTrees
	)
	{
		MetadataReference[] references = GetBaseReferences();

		return CSharpCompilation.Create(
			assemblyName: assemblyName,
			syntaxTrees: syntaxTrees,
			references: references,
			options: new CSharpCompilationOptions(outputKind)
		);
	}

	private static MetadataReference[] GetBaseReferences()
	{
		string directory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

		string[] locations = new string[]
		{
			typeof(Console).Assembly.Location,
			typeof(object).Assembly.Location,
			typeof(File).Assembly.Location,
			typeof(BigInteger).Assembly.Location,
			typeof(Enumerable).Assembly.Location,
			typeof(List<>).Assembly.Location,
			Path.Combine(directory, "System.Runtime.dll"),
#pragma warning disable ZUBR0001 // Type or member is obsolete
			typeof(InternalInheritAttribute).Assembly.Location
#pragma warning restore ZUBR0001 // Type or member is obsolete
		};

		List<MetadataReference> references = new(locations.Length);

		foreach (string location in locations)
		{
			references.Add(MetadataReference.CreateFromFile(location));
		}

		return references.ToArray();
	}
}
