using System;
using System.Diagnostics;
using System.Globalization;
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
					return new(SyntaxKind.NumericLiteralToken, ReadNumericLiteral(out object? decimalValue), _tokenStartPos, decimalValue);
				}

				_reader.Move();

				if (_reader.Peek() == '.')
				{
					_reader.Move();
					return new(SyntaxKind.DotDotToken, "..", _tokenStartPos);
				}

				return new(SyntaxKind.DotToken, ".", _tokenStartPos);

			case '\"':
				return new(SyntaxKind.StringLiteralToken, ReadStringLiteral(out string? stringValue), _tokenStartPos, stringValue);

			case '\'':
				return new(SyntaxKind.CharLiteralToken, ReadCharLiteral(out char charValue), _tokenStartPos, charValue);

			case >= '0' and <= '9':
				return new(SyntaxKind.NumericLiteralToken, ReadNumericLiteral(out object? numericValue), _tokenStartPos, numericValue);

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

	private string ReadNumericLiteral(out object? value)
	{
		bool hasPoint = false;
		bool allowUnderscore = false;

		NumberType type = NumberType.Int;

		while (true)
		{
			char c = _reader.Peek();

			// Eof
			if (c == SourceReader.InvalidChar)
			{
				break;
			}

			if (SyntaxFacts.IsDigit(c))
			{
				allowUnderscore = true;
				AppendAndMove(c);
				continue;
			}

			if (c == '.')
			{
				if (hasPoint)
				{
					// Ill-formed literal
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return ToStringAndClear();
				}

				allowUnderscore = false;
				hasPoint = true;
				AppendAndMove(c);
				type = NumberType.Double;
				continue;
			}

			if (c == '_')
			{
				if (!allowUnderscore)
				{
					// Ill-formed literal.
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return ToStringAndClear();
				}

				char next = _reader.Peek(1);

				if (next == SourceReader.InvalidChar)
				{
					break;
				}

				if (!SyntaxFacts.IsDigit(next))
				{
					// Ill-formed literal.
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return ToStringAndClear();
				}

				AppendAndMove(c);
				AppendAndMove(next);

				continue;
			}

			if (c == 'f' || c == 'F')
			{
				AppendAndMove(c);
				type = NumberType.Float;

				// End of the literal.
				break;
			}

			if (c == 'd' || c == 'D')
			{
				AppendAndMove(c);
				type = NumberType.Double;

				// End of the literal.
				break;
			}

			if (c == 'm' || c == 'M')
			{
				AppendAndMove(c);
				type = NumberType.Decimal;

				// End of the literal.
				break;
			}

			if (!hasPoint)
			{
				if (c == 'l' || c == 'L')
				{
					AppendAndMove(c);

					c = _reader.Peek();

					if (c == 'u' || c == 'U')
					{
						AppendAndMove(c);
						type = NumberType.ULong;
					}
					else
					{
						type = NumberType.Long;
					}

					// End of the literal.
					break;
				}
				else if (c == 'u' || c == 'U')
				{
					AppendAndMove(c);

					c = _reader.Peek();

					if (c == 'l' || c == 'L')
					{
						AppendAndMove(c);
						type = NumberType.Long;
					}
					else
					{
						type = NumberType.UInt;
					}

					// End of the literal.
					break;
				}
			}

			// End of the literal.
			break;
		}

		string literal = ToStringAndClear();
		TryParsePrimitiveValue(literal, type, out value);

		return literal;
	}

	private bool TryParsePrimitiveValue(string literal, NumberType type, out object? value)
	{
		switch (type)
		{
			case NumberType.Int:
				if (!int.TryParse(literal, NumberStyles.None, CultureInfo.InvariantCulture, out int intValue))
				{
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return false;
				}

				value = intValue;
				return true;

			case NumberType.Long:
				if (!long.TryParse(literal, NumberStyles.None, CultureInfo.InvariantCulture, out long longValue))
				{
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return false;
				}

				value = longValue;
				return true;

			case NumberType.Short:
				if (!short.TryParse(literal, NumberStyles.None, CultureInfo.InvariantCulture, out short shortValue))
				{
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return false;
				}

				value = shortValue;
				return true;

			case NumberType.Byte:
				if (!byte.TryParse(literal, NumberStyles.None, CultureInfo.InvariantCulture, out byte byteValue))
				{
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return false;
				}

				value = byteValue;
				return true;

			case NumberType.UInt:
				if (!uint.TryParse(literal, NumberStyles.None, CultureInfo.InvariantCulture, out uint uintValue))
				{
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return false;
				}

				value = uintValue;
				return true;

			case NumberType.ULong:
				if (!ulong.TryParse(literal, NumberStyles.None, CultureInfo.InvariantCulture, out ulong ulongValue))
				{
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return false;
				}

				value = ulongValue;
				return true;

			case NumberType.UShort:
				if (!ushort.TryParse(literal, NumberStyles.None, CultureInfo.InvariantCulture, out ushort ushortValue))
				{
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return false;
				}

				value = ushortValue;
				return true;

			case NumberType.SByte:
				if (!sbyte.TryParse(literal, NumberStyles.None, CultureInfo.InvariantCulture, out sbyte sbyteValue))
				{
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return false;
				}

				value = sbyteValue;
				return true;

			case NumberType.Float:
				if (!float.TryParse(literal, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out float floatValue))
				{
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return false;
				}

				value = floatValue;
				return true;

			case NumberType.Double:
				if (!double.TryParse(literal, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out double doubleValue))
				{
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return false;
				}

				value = doubleValue;
				return true;

			case NumberType.Decimal:
				if (!decimal.TryParse(literal, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out decimal decimalValue))
				{
					AddError(ErrorCode.ERR_SyntaxError);
					value = null;
					return false;
				}

				value = decimalValue;
				return true;

			default:
				throw new UnreachableException();
		}
	}

	private string ReadStringLiteral(out string? value)
	{
		string literal = ReadCharOrStringLiteral();
		ReadOnlySpan<char> span = literal.AsSpan();

		// Ill-formed literal.
		if (span.Length == 0 || span.Length == 1 || span[^1] != '\"')
		{
			value = default;
			return literal;
		}

		// Empty string.
		if (span.Length == 2)
		{
			value = string.Empty;
			return literal;
		}

		// Skip start and end quotes.
		span = span[1..^1];

		for (int i = 0; i < span.Length; i++)
		{
			// New line character in string, invalid.
			if (SyntaxFacts.IsNewLine(span[i]))
			{
				AddError(ErrorCode.ERR_SyntaxError);
				_builder.Clear();
				value = null;
				return literal;
			}

			if (span[i] == '\\')
			{
				int next = i + 1;

				// Empty escape character, invalid.
				if(next > span.Length - 1)
				{
					AddError(ErrorCode.ERR_SyntaxError);
					_builder.Clear();
					value = null;
					return literal;
				}

				char escaped = GetEscapedChar(span[next]);

				if (escaped == SourceReader.InvalidChar)
				{
					// Invalid escape character.
					AddError(ErrorCode.ERR_SyntaxError);
					_builder.Clear();
					value = null;
					return literal;
				}

				_builder.Append(escaped);

				continue;
			}

			_builder.Append(span[i]);
		}

		value = ToStringAndClear();
		return literal;
	}

	private string ReadCharLiteral(out char value)
	{
		string literal = ReadCharOrStringLiteral();
		ReadOnlySpan<char> span = literal.AsSpan();

		// Ill-formed literal.
		if (span.Length == 0 || span.Length == 1 || span[^1] != '\'')
		{
			value = default;
			return literal;
		}

		// Empty char, invalid.
		if (span.Length == 2)
		{
			AddError(ErrorCode.ERR_SyntaxError);
			value = default;
			return literal;
		}

		// Skip start and end quotes.
		span = span[1..^1];

		if (span.Length == 1)
		{
			// Unescaped slash, invalid.
			if (span[0] == '\\')
			{
				AddError(ErrorCode.ERR_SyntaxError);
				value = default;
				return literal;
			}

			if (SyntaxFacts.IsNewLine(span[0]) || !char.IsAscii(span[0]))
			{
				AddError(ErrorCode.ERR_SyntaxError);
				value = default;
				return literal;
			}

			value = span[0];
		}
		else if (span.Length == 2)
		{
			if (span[0] == '\\')
			{
				char escaped = GetEscapedChar(span[1]);

				if (escaped == SourceReader.InvalidChar)
				{
					// Invalid escape character.
					AddError(ErrorCode.ERR_SyntaxError);
					value = default;
					return literal;
				}

				value = escaped;
			}
			else
			{
				AddError(ErrorCode.ERR_SyntaxError);
				value = default;
			}
		}
		else
		{
			AddError(ErrorCode.ERR_SyntaxError);
			value = default;
		}

		return literal;
	}

	private string ReadCharOrStringLiteral()
	{
		char qoute = _reader.Read();

		// Append the quote.
		_builder.Append(qoute);

		while (true)
		{
			char c = _reader.Peek();

			// Eof
			if (c == SourceReader.InvalidChar)
			{
				AddError(ErrorCode.ERR_UnexpectedEndOfFile);
				break;
			}

			_reader.Move();

			_builder.Append(c);

			// Escaped quote, so don't end the literal.
			if (c == '\\' && _reader.Peek() == qoute)
			{
				_builder.Append(_reader.Read());
				continue;
			}

			// End of the literal.
			if (c == qoute)
			{
				break;
			}
		}

		return ToStringAndClear();
	}

	private static char GetEscapedChar(char c)
	{
		return c switch
		{
			'\\' or '"' or '\'' => c,
			'r' => '\u000d',
			'n' => '\u000a',
			't' => '\u0009',
			_ => SourceReader.InvalidChar, // Unknown escape character
		};
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

	private enum NumberType : byte
	{
		Int,

		Long,

		Short,

		Byte,

		UInt,

		ULong,

		UShort,

		SByte,

		Float,

		Double,

		Decimal
	}
}
