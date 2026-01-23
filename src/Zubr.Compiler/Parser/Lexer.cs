using System.Text;
using Zubr.Compiler.Diagnostics;

namespace Zubr.Compiler.Parser;

internal sealed partial class Lexer
{
	private readonly SourceReader _reader;
	private readonly StringBuilder _builder;

	private int _tokenStartPos;

	internal Lexer(SourceReader reader)
	{
		_reader = reader;
		_builder = new(32);
	}

	public SyntaxToken Lex()
	{
		ReadTrivia();
		return ReadToken();
	}

	private SyntaxToken ReadToken()
	{
		_tokenStartPos = _reader.Position;

		char c = _reader.Peek();

		switch(c)
		{
			case '+':
				_reader.Move();

				if(_reader.Peek() == '+')
				{
					_reader.Move();

					return new(SyntaxKind.PlusPlusToken, "++", _tokenStartPos);
				}

				if(_reader.Peek() == '=')
				{
					_reader.Move();

					return new(SyntaxKind.PlusEqualsToken, "+=", _tokenStartPos);
				}

				return new(SyntaxKind.PlusToken, "+", _tokenStartPos);

			case '-':
				_reader.Move();

				if (_reader.Peek() == '-')
				{
					_reader.Move();

					return new(SyntaxKind.MinusMinusToken, "--", _tokenStartPos);
				}

				if (_reader.Peek() == '=')
				{
					_reader.Move();

					return new(SyntaxKind.MinusEqualsToken, "-=", _tokenStartPos);
				}

				return new(SyntaxKind.PlusToken, "-", _tokenStartPos);

			case '=':
				_reader.Move();

				if(_reader.Peek() == '=')
				{
					_reader.Move();

					return new(SyntaxKind.EqualsEqualsToken, "==", _tokenStartPos);
				}

				return new(SyntaxKind.EqualsToken, "=", _tokenStartPos);

			case '&':
				_reader.Move();

				if (_reader.Peek() == '&')
				{
					_reader.Move();

					return new(SyntaxKind.AmpersandAmpersandToken, "&&", _tokenStartPos);
				}

				if (_reader.Peek() == '=')
				{
					_reader.Move();

					return new(SyntaxKind.AmpersandEqualsToken, "&=", _tokenStartPos);
				}

				return new(SyntaxKind.AmpersandToken, "&", _tokenStartPos);

			case '|':
				_reader.Move();

				if (_reader.Peek() == '|')
				{
					_reader.Move();

					return new(SyntaxKind.BarBarToken, "||", _tokenStartPos);
				}

				if (_reader.Peek() == '=')
				{
					_reader.Move();

					return new(SyntaxKind.BarEqualsToken, "|=", _tokenStartPos);
				}

				return new(SyntaxKind.BarToken, "|", _tokenStartPos);

			case '*':
				_reader.Move();

				if (_reader.Peek() == '=')
				{
					_reader.Move();

					return new(SyntaxKind.AsteriskEqualsToken, "*=", _tokenStartPos);
				}

				return new(SyntaxKind.AsteriskToken, "*", _tokenStartPos);

			case '^':
				_reader.Move();

				if (_reader.Peek() == '=')
				{
					_reader.Move();

					return new(SyntaxKind.CaretEqualsToken, "^=", _tokenStartPos);
				}

				return new(SyntaxKind.CaretToken, "^", _tokenStartPos);

			case '%':
				_reader.Move();

				if (_reader.Peek() == '=')
				{
					_reader.Move();

					return new(SyntaxKind.PercentEqualsToken, "%=", _tokenStartPos);
				}

				return new(SyntaxKind.PercentToken, "%", _tokenStartPos);

			case '/':
				_reader.Move();

				if (_reader.Peek() == '=')
				{
					_reader.Move();

					return new(SyntaxKind.SlashEqualsToken, "/=", _tokenStartPos);
				}

				return new(SyntaxKind.SlashToken, "/", _tokenStartPos);

			case '(':
				_reader.Move();
				return new(SyntaxKind.OpenParenToken, "(", _tokenStartPos);

			case ')':
				_reader.Move();
				return new(SyntaxKind.CloseParenToken, ")", _tokenStartPos);

			case '[':
				_reader.Move();
				return new(SyntaxKind.OpenBracketToken, "[", _tokenStartPos);

			case ']':
				_reader.Move();
				return new(SyntaxKind.CloseBracketToken, "]", _tokenStartPos);

			case '{':
				_reader.Move();
				return new(SyntaxKind.OpenBraceToken, "{", _tokenStartPos);

			case '}':
				_reader.Move();
				return new(SyntaxKind.CloseBraceToken, "}", _tokenStartPos);

			case ',':
				_reader.Move();
				return new(SyntaxKind.CommaToken, ",", _tokenStartPos);

			case ';':
				_reader.Move();
				return new(SyntaxKind.SemicolonToken, ";", _tokenStartPos);

			case ':':
				_reader.Move();

				if(_reader.Peek() == ':')
				{
					return new(SyntaxKind.ColonColonToken, "::", _tokenStartPos);
				}

				return new(SyntaxKind.ColonToken, ":", _tokenStartPos);

			case '!':
				_reader.Move();
				return new(SyntaxKind.ExclamationToken, "!", _tokenStartPos);

			case '>':
				_reader.Move();

				if (_reader.Peek() == '=')
				{
					_reader.Move();
					return new(SyntaxKind.GreaterThanOrEqualToken, ">=", _tokenStartPos);
				}

				return new(SyntaxKind.GreaterThanToken, ">", _tokenStartPos);

			case '<':
				_reader.Move();

				if (_reader.Peek() == '=')
				{
					_reader.Move();
					return new(SyntaxKind.LessThanOrEqualToken, "<=", _tokenStartPos);
				}

				return new(SyntaxKind.LessThanToken, "<", _tokenStartPos);

			case '?':
				_reader.Move();
				return new(SyntaxKind.QuestionToken, "?", _tokenStartPos);

			case '.':
				if (SyntaxFacts.IsDigit(_reader.Peek(1)))
				{
					return new(SyntaxKind.NumericLiteralToken, ReadNumericLiteral(), _tokenStartPos);
				}

				_reader.Move();

				if (_reader.Peek() == '.')
				{
					_reader.Move();
					return new(SyntaxKind.DotDotToken, "..", _tokenStartPos);
				}

				return new(SyntaxKind.DotToken, ".", _tokenStartPos);

			case '\"':
				return new(SyntaxKind.StringLiteralToken, ReadStringLiteral(), _tokenStartPos);

			case '\'':
				return new(SyntaxKind.CharLiteralToken, ReadCharLiteral(), _tokenStartPos);

			case >= '0' and <= '9':
				return new(SyntaxKind.NumericLiteralToken, ReadNumericLiteral(), _tokenStartPos);

			case '_':
			case (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'):
				return ReadIdentifierOrKeyword();

			case SourceReader.InvalidChar:
				return new(SyntaxKind.EOF, string.Empty, _tokenStartPos);

			default:
				_reader.Move();
				AddError(ErrorCode.ERR_UnexpectedCharacter);
				return new(SyntaxKind.None, string.Empty, _tokenStartPos);
		}
	}

	private SyntaxToken ReadIdentifierOrKeyword()
	{
		string identifier = ReadIdentifier();

		SyntaxKind keyword = SyntaxFacts.GetKeywordKind(identifier);

		if(keyword != SyntaxKind.None)
		{
			return new(keyword, identifier, _tokenStartPos);
		}

		return new(SyntaxKind.IdentifierToken, identifier, _tokenStartPos);
	}

	private string ReadIdentifier()
	{
		// Add the first char.
		char c = _reader.Peek();
		_builder.Append(c);
		_reader.Move();

		while ((c = _reader.Peek()) != SourceReader.InvalidChar)
		{
			if (SyntaxFacts.IsValidIdentifierCharacter(c))
			{
				_builder.Append(c);
				_reader.Move();
			}
			else
			{
				// End of indentifier.
				break;
			}
		}

		return ToStringAndClear();
	}

	private string ReadNumericLiteral()
	{
		bool hasPoint = false;
		bool allowUnderscore = false;

		char c;

		while ((c = _reader.Peek()) != SourceReader.InvalidChar)
		{
			if(SyntaxFacts.IsDigit(c))
			{
				allowUnderscore = true;
				AppendAndMove(c);
			}
			else if (c == '.')
			{
				if (hasPoint)
				{
					// Ill-formed literal
					break;
				}

				allowUnderscore = false;
				hasPoint = true;
				AppendAndMove(c);
			}
			else if (c == '_')
			{
				if (!allowUnderscore || !SyntaxFacts.IsDigit(_reader.Peek(1)))
				{
					// Ill-formed literal.
					break;
				}

				AppendAndMove(c); ;
			}
			else if (c == 'f' || c == 'F')
			{
				AppendAndMove(c);
			}
			else if (c == 'd' || c == 'D')
			{
				AppendAndMove(c);
			}
			else if (c == 'm' || c == 'M')
			{
				AppendAndMove(c);
			}
			else if (!hasPoint)
			{
				if (c == 'l' || c == 'L')
				{
					AppendAndMove(c);

					c = _reader.Peek();

					if (c == 'u' || c == 'U')
					{
						AppendAndMove(c);
					}
				}
				else if (c == 'u' || c == 'U')
				{
					AppendAndMove(c);

					c = _reader.Peek();

					if(c == 'l' || c == 'L')
					{
						AppendAndMove(c);
					}
				}
			}
		}

		return ToStringAndClear();
	}

	private string ReadStringLiteral()
	{
		char quote = _reader.Read();
		_builder.Append(quote);

		char c;

		while ((c = _reader.Peek()) != SourceReader.InvalidChar)
		{
			_reader.Move();

			// End of the literal.
			if (c == '"')
			{
				_builder.Append(c);
				break;
			}

			if (SyntaxFacts.IsNewLine(c))
			{
				// Ill-formed literal.
				break;
			}

			// Escaped character.
			if (c == '\\')
			{
				_reader.Move();

				char escaped = GetEscapedChar();

				if (escaped == SourceReader.InvalidChar)
				{
					break;
				}

				_builder.Append(escaped);
			}
		}

		return ToStringAndClear();
	}

	private string ReadCharLiteral()
	{
		// Append the quote.
		_builder.Append(_reader.Read());

		char c = _reader.Read();

		if (c == '\'')
		{
			// Empty char, invalid.
			_builder.Append(c);
			return ToStringAndClear();
		}

		if(c == '\\')
		{
			char escaped = GetEscapedChar();

			if (escaped == SourceReader.InvalidChar)
			{
				return ToStringAndClear();
			}

			_builder.Append(escaped);

			c = _reader.Read();

			if(c != '\'')
			{
				return ToStringAndClear();
			}

			_builder.Append(c);
		}
		else if(SyntaxFacts.IsNewLine(c))
		{
			// Ill-formed literal.
			return ToStringAndClear();
		}
		else
		{
			_builder.Append(c);
		}

		return ToStringAndClear();
	}

	private char GetEscapedChar()
	{
		char c = _reader.Read();

		switch(c)
		{
			case '\\':
			case '"':
			case '\'':
				return c;

			case '\r':
				return '\u000d';

			case '\n':
				return '\u000a';

			case '\t':
				return '\u0009';

			default:
				// Unknown escape character
				return SourceReader.InvalidChar;
		}
	}

	private string ToStringAndClear()
	{
		string str = _builder.ToString();
		_builder.Clear();
		return str;
	}

	private void AppendAndMove(char c)
	{
		_builder.Append(c);
		_reader.Move();
	}

	private void ReadTrivia()
	{
		char c;

		while((c = _reader.Peek()) != SourceReader.InvalidChar)
		{
			if(char.IsWhiteSpace(c))
			{
				_reader.Move();
				continue;
			}

			if(c == '/')
			{
				// Single-line comment.
				if(_reader.Peek(1) == '/')
				{
					_reader.Move(2);
					ReadUntilNewLine();

					continue;
				}
				else
				{
					// Not a comment.
					return;
				}
			}

			if(c == '*')
			{
				// Multi-line comment.
				if(_reader.Peek(1) == '*')
				{
					_reader.Move(2);
					ReadUntilMultiLineCommentEnd();

					continue;
				}
				else
				{
					// Not a comment.
					return;
				}
			}

			// End of trivia.
			break;
		}
	}

	private void ReadUntilNewLine()
	{
		char c;

		while((c = _reader.Peek()) != SourceReader.InvalidChar)
		{
			_reader.Move();

			if (c == '\r')
			{
				// Eat the new line too.
				if(_reader.Peek() == '\n')
				{
					_reader.Move();
				}

				return;
			}

			if(SyntaxFacts.IsNewLine(c))
			{
				return;
			}
		}
	}

	private void ReadUntilMultiLineCommentEnd()
	{
		char c;

		while((c = _reader.Peek()) != SourceReader.InvalidChar)
		{
			_reader.Move();

			// End of the multi-line comment.
			if(c == '*' && _reader.Peek() == '*')
			{
				_reader.Move();
				return;
			}
		}
	}
}
