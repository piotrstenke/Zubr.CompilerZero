using Zubr.Compiler;
using Zubr.Compiler.CSharp;
using Zubr.Compiler.Diagnostics;

using Zubr.Compiler.Parser;

Console.WriteLine("Zubr programming language exaple.");
Console.WriteLine();
Console.WriteLine("Tokens:");
Console.WriteLine();

SourceText source = SourceText.FromSource(
"""
use System;
use System.Collections.Generic;
use System.Collections.Generic.Console as console;

// This is a comment

**

This is also a comment

**

pub void print(string message = "")
{
	//Console.Write(message);
}

module top;

void println(mut string message, int a)
{
	//Console.WriteLine(message);
}

module Hello;

scoped open class Program
{
	int main()
	{
		//return 1;
	}
}

priv struct Test
{
}

"""
);

Lexer lexer = new(source.GetSourceReader());

SyntaxToken token;

int count = 0;

while((token = lexer.Lex()).Kind != SyntaxKind.EOF)
{
	if(token.Kind == SyntaxKind.None)
	{
		continue;
	}

	Console.WriteLine($"{count++}: {token}");
}

Diagnostic[]? errors = lexer.GetErrors();

if (errors is not null)
{
	Console.WriteLine();
	Console.WriteLine("Lexing failed with errors:");
	Console.WriteLine();

	for (int i = 0; i < errors.Length; i++)
	{
		Console.WriteLine($"{errors[i].Code} at position {errors[i].Position}");
	}
}
else
{
	SyntaxTree tree = SyntaxTree.Parse(source);

	CSharpTranslator translator = CSharpTranslator.Create();
	var compiled = translator.Translate(tree);

	Console.WriteLine();
	Console.WriteLine("Compiled Zubr code to C#:");
	Console.WriteLine();

	Console.WriteLine(compiled.ToString());
}

Console.ReadKey();
