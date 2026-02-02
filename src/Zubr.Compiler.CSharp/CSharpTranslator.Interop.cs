using Microsoft.CodeAnalysis.CSharp;
using System;

using Sharp = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Zubr.Compiler.CSharp;

partial class CSharpTranslator
{
	private static class Interop
	{
		public static Sharp.AttributeListSyntax InternalInheritAttribute()
		{
			return Attribute("zubr", "interop", "csharp", "InternalInherit");
		}

		public static Sharp.AttributeListSyntax MustOverrideAttribute()
		{
			return Attribute("zubr", "interop", "csharp", "MustOverride");
		}

		public static Sharp.AttributeListSyntax FlagsAttribute()
		{
			return Attribute("System", "Flags");
		}

		private static Sharp.AttributeListSyntax Attribute(params ReadOnlySpan<string> names)
		{
			return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(Expressions.GlobalQualifiedName(names))));
		}
	}
}
