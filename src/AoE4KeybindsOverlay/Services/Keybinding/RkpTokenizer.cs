using System.Runtime.CompilerServices;
using System.Text;

namespace AoE4KeybindsOverlay.Services.Keybinding;

/// <summary>
/// Represents the type of a token produced by the <see cref="RkpTokenizer"/>.
/// </summary>
public enum RkpTokenType
{
    /// <summary>An identifier such as a field name or keyword (e.g., "profile", "command").</summary>
    Identifier,

    /// <summary>A double-quoted string literal (e.g., "zoom_in").</summary>
    StringLiteral,

    /// <summary>An integer literal, possibly negative (e.g., -1, 42).</summary>
    IntegerLiteral,

    /// <summary>A boolean literal (true or false).</summary>
    BooleanLiteral,

    /// <summary>The equals sign '='.</summary>
    Equals,

    /// <summary>An opening brace '{'.</summary>
    OpenBrace,

    /// <summary>A closing brace '}'.</summary>
    CloseBrace,

    /// <summary>A comma ','.</summary>
    Comma,

    /// <summary>End of input.</summary>
    EOF
}

/// <summary>
/// Represents a single token from the .rkp file with its type, value, and source location.
/// </summary>
/// <param name="Type">The type of this token.</param>
/// <param name="Value">The string value of this token. For string literals, this is the unquoted content.</param>
/// <param name="Line">The 1-based line number where this token appears.</param>
public readonly record struct RkpToken(RkpTokenType Type, string Value, int Line)
{
    /// <inheritdoc/>
    public override string ToString() => Type switch
    {
        RkpTokenType.StringLiteral => $"\"{Value}\" (line {Line})",
        RkpTokenType.EOF => $"EOF (line {Line})",
        _ => $"{Value} (line {Line})"
    };
}

/// <summary>
/// Tokenizes .rkp (Relic Key-binding Profile) files into a stream of <see cref="RkpToken"/> values.
/// </summary>
/// <remarks>
/// <para>
/// The tokenizer handles the Relic Lua-like format used by Age of Empires IV for keybinding profiles.
/// It recognizes identifiers, string literals, integer literals (including negative values), boolean
/// literals, and structural tokens (equals, braces, commas).
/// </para>
/// <para>
/// This implementation reads the entire input into memory and processes it character-by-character
/// for efficiency on files up to 26,000+ lines. Whitespace and blank lines are skipped automatically.
/// </para>
/// </remarks>
public sealed class RkpTokenizer
{
    private readonly string _source;
    private int _position;
    private int _line;

    /// <summary>
    /// Initializes a new <see cref="RkpTokenizer"/> for the given source text.
    /// </summary>
    /// <param name="source">The full text content of an .rkp file.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    public RkpTokenizer(string source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _position = 0;
        _line = 1;
    }

    /// <summary>
    /// Tokenizes the entire input and returns all tokens as a list.
    /// The list always ends with an <see cref="RkpTokenType.EOF"/> token.
    /// </summary>
    /// <returns>A list of all tokens in the input, terminated by an EOF token.</returns>
    /// <exception cref="RkpParseException">The input contains an unterminated string or an unexpected character.</exception>
    public List<RkpToken> TokenizeAll()
    {
        // Pre-allocate a reasonably sized list; a 26k-line file produces roughly 60k-80k tokens.
        var tokens = new List<RkpToken>(Math.Max(256, _source.Length / 4));

        while (true)
        {
            var token = NextToken();
            tokens.Add(token);
            if (token.Type == RkpTokenType.EOF)
                break;
        }

        return tokens;
    }

    /// <summary>
    /// Reads and returns the next token from the input. Returns an EOF token when the input is exhausted.
    /// </summary>
    /// <exception cref="RkpParseException">The input contains an unterminated string or an unexpected character.</exception>
    private RkpToken NextToken()
    {
        SkipWhitespace();

        if (_position >= _source.Length)
            return new RkpToken(RkpTokenType.EOF, string.Empty, _line);

        var ch = _source[_position];

        return ch switch
        {
            '=' => SingleCharToken(RkpTokenType.Equals, "="),
            '{' => SingleCharToken(RkpTokenType.OpenBrace, "{"),
            '}' => SingleCharToken(RkpTokenType.CloseBrace, "}"),
            ',' => SingleCharToken(RkpTokenType.Comma, ","),
            '"' => ReadString(),
            '-' => ReadNegativeIntegerOrError(),
            _ when char.IsDigit(ch) => ReadInteger(),
            _ when IsIdentifierStart(ch) => ReadIdentifierOrKeyword(),
            _ => throw new RkpParseException($"Unexpected character '{ch}'", _line)
        };
    }

    /// <summary>
    /// Creates a single-character token and advances the position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RkpToken SingleCharToken(RkpTokenType type, string value)
    {
        var token = new RkpToken(type, value, _line);
        _position++;
        return token;
    }

    /// <summary>
    /// Reads a double-quoted string literal. The quotes are consumed but not included in the token value.
    /// </summary>
    private RkpToken ReadString()
    {
        var startLine = _line;
        _position++; // skip opening quote

        var start = _position;
        while (_position < _source.Length && _source[_position] != '"')
        {
            if (_source[_position] == '\n')
                _line++;
            _position++;
        }

        if (_position >= _source.Length)
            throw new RkpParseException("Unterminated string literal", startLine);

        var value = _source.Substring(start, _position - start);
        _position++; // skip closing quote

        return new RkpToken(RkpTokenType.StringLiteral, value, startLine);
    }

    /// <summary>
    /// Reads a negative integer literal (starting with '-').
    /// </summary>
    private RkpToken ReadNegativeIntegerOrError()
    {
        var startLine = _line;

        // Check that the next character after '-' is a digit.
        if (_position + 1 < _source.Length && char.IsDigit(_source[_position + 1]))
        {
            var start = _position;
            _position++; // skip '-'
            while (_position < _source.Length && char.IsDigit(_source[_position]))
                _position++;

            var value = _source.Substring(start, _position - start);
            return new RkpToken(RkpTokenType.IntegerLiteral, value, startLine);
        }

        throw new RkpParseException("Unexpected character '-' (not followed by a digit)", startLine);
    }

    /// <summary>
    /// Reads a non-negative integer literal.
    /// </summary>
    private RkpToken ReadInteger()
    {
        var startLine = _line;
        var start = _position;

        while (_position < _source.Length && char.IsDigit(_source[_position]))
            _position++;

        var value = _source.Substring(start, _position - start);
        return new RkpToken(RkpTokenType.IntegerLiteral, value, startLine);
    }

    /// <summary>
    /// Reads an identifier or a boolean keyword (true/false).
    /// </summary>
    private RkpToken ReadIdentifierOrKeyword()
    {
        var startLine = _line;
        var start = _position;

        while (_position < _source.Length && IsIdentifierChar(_source[_position]))
            _position++;

        var value = _source.Substring(start, _position - start);

        if (value is "true" or "false")
            return new RkpToken(RkpTokenType.BooleanLiteral, value, startLine);

        return new RkpToken(RkpTokenType.Identifier, value, startLine);
    }

    /// <summary>
    /// Skips whitespace characters (spaces, tabs, carriage returns, newlines) and tracks line numbers.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipWhitespace()
    {
        while (_position < _source.Length)
        {
            var ch = _source[_position];
            if (ch == '\n')
            {
                _line++;
                _position++;
            }
            else if (ch is ' ' or '\t' or '\r')
            {
                _position++;
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// Returns true if the character can start an identifier (letter or underscore).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIdentifierStart(char ch) =>
        ch is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '_';

    /// <summary>
    /// Returns true if the character can appear in an identifier (letter, digit, or underscore).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIdentifierChar(char ch) =>
        ch is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '_';
}
