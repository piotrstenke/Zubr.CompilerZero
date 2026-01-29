using System.Diagnostics;
using System.Reflection;
using System.Text;
using Zubr;
using Zubr.Compiler;
using Zubr.Compiler.CSharp;
using Zubr.Compiler.Diagnostics;
using Zubr.Compiler.Parser;
using Zubr.Compiler.Syntax;

Console.WriteLine();
Console.WriteLine("------------------------------------------------");
Console.WriteLine("Zubr programming language sample.");
Console.WriteLine("------------------------------------------------");
Console.WriteLine();
//Console.WriteLine("Path to .zr file:");
//Console.WriteLine("(leave empty to use default path)");
//Console.WriteLine();

string? path = "code_sample.zr"; //= Console.ReadLine();

//if(string.IsNullOrWhiteSpace(path))
//{
//	path = "code_sample.zr";
//}

path = Path.GetFullPath(path);

Console.WriteLine();
Console.WriteLine($"Reading Zubr file at path: {path}");
Console.WriteLine();

string text = File.ReadAllText(path);

Console.WriteLine();
Console.WriteLine("------------------------------------------------");
Console.WriteLine("Tokens:");
Console.WriteLine("------------------------------------------------");
Console.WriteLine();

SourceText source = SourceText.FromSource(text);

Stopwatch watch = new();
watch.Start();

Lexer lexer = new(source.GetSourceReader());

int count = 0;
Token token;

List<Token> tokens = new();

while ((token = lexer.Lex()).Kind != TokenKind.EOF)
{
	if (token.Kind == TokenKind.None)
	{
		continue;
	}

	Console.WriteLine($"{count++}: {token}");
	tokens.Add(token);
}

watch.Stop();

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

	Console.ReadKey();
	return;
}

Console.WriteLine();
Console.WriteLine($"Lexing took {watch.ElapsedMilliseconds}ms");

// Add EOF
tokens.Add(token);

watch.Restart();

SourceParser parser = new(tokens.ToArray());
CompilationUnitSyntax root = parser.ParseCompilationUnit();

watch.Stop();

errors = parser.GetDiagnostics();

if (errors is not null)
{
	Console.WriteLine();
	Console.WriteLine("Parsing failed with errors:");
	Console.WriteLine();

	for (int i = 0; i < errors.Length; i++)
	{
		Console.WriteLine($"{errors[i].Code} at position {errors[i].Position}");
	}

	Console.ReadKey();
	return;
}

Console.WriteLine();
Console.WriteLine($"Parsing took {watch.ElapsedMilliseconds}ms");

CSharpTranslator translator = CSharpTranslator.Create();
var tree = translator.Translate(new SyntaxTree(root, Encoding.UTF8));

Console.WriteLine();
Console.WriteLine("------------------------------------------------");
Console.WriteLine("Compiled Zubr code to C#:");
Console.WriteLine("------------------------------------------------");
Console.WriteLine();

Console.WriteLine(tree.ToString());

var compilation = RoslynUtilities.CreateCompilation(tree);

using MemoryStream stream = new();

var result = compilation.Emit(stream, options: new()
{});

if (!result.Success)
{
	Console.WriteLine();
	Console.WriteLine("------------------------------------------------");
	Console.WriteLine("Emit failed with errors:");
	Console.WriteLine("------------------------------------------------");
	Console.WriteLine();

	foreach (var diag in result.Diagnostics)
	{
		Console.WriteLine(diag.ToString());
	}

	Console.ReadKey();
	return;
}

Console.WriteLine();
Console.WriteLine("------------------------------------------------");
Console.WriteLine("Calling main() in Zubr code...");
Console.WriteLine("------------------------------------------------");
Console.WriteLine();

Assembly assembly = Assembly.Load(stream.ToArray());

Type? type = assembly.GetType("TopLevel");
MethodInfo? method = type?.GetMethod("main");

if (method is null)
{
	Console.WriteLine("Could not find the main() function.");
	Console.ReadKey();
	return;
}

object? output = method.Invoke(null, null);

if (output is not null)
{
	Console.WriteLine();
	Console.WriteLine("Zubr code resulted in:");
	Console.WriteLine();
	Console.WriteLine(output.ToString());
}

Console.ReadKey();
