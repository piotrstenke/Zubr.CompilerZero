using Zubr.Compiler;
using Zubr.Compiler.CSharp;
using Zubr.Compiler.Diagnostics;

using Zubr.Compiler.Parser;

Console.WriteLine("Zubr programming language example.");
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

void println<T>(mut T message, int a) where
	T : struct
{
	//Console.WriteLine(message);
}

module Hello;

scoped open class Program
{
	int main()
	{
		bool flag = true;

		if(flag)
		{
			//println("Hello");
		}
		elif(true)
		{
		}
		elif(false)
		{
		}
		else
		{
		}

		while(true)
		{
			int a = 2;
		}

		do
		{
			string b = "hello \tthere";
			char c = '5';

			bool d = (1 + -2.2) * 3 == (int)7 << 2 && foo(4) < 10;
		}
		while(true);

		for (int a : collection)
		{
		}

		return 1;
	}

	priv int foo(int a)
	{
		return ++a;
	}
}

priv struct Test<T, U> where
	T : Clone, U,
	U : Clone, class,
{
}

//pub trait Clone
//{
//	self clone();
//}

//give Clone to Program
//{
//	Program clone()
//	{
//		return self;
//	}
//}

"""
);

Lexer lexer = new(source.GetSourceReader());

SyntaxToken token;

int count = 0;

int b = count;

while ((token = lexer.Lex()).Kind != SyntaxKind.EOF)
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
