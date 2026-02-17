namespace AoE4KeybindsOverlay.Services.Keybinding;

#region Intermediate AST types

/// <summary>
/// Base type for all values in the parsed .rkp AST.
/// </summary>
public abstract record RkpValue;

/// <summary>
/// A string value from the .rkp file (e.g., <c>"zoom_in"</c>).
/// </summary>
/// <param name="Value">The unquoted string content.</param>
public sealed record RkpString(string Value) : RkpValue;

/// <summary>
/// An integer value from the .rkp file (e.g., <c>-1</c>).
/// </summary>
/// <param name="Value">The integer value.</param>
public sealed record RkpInteger(int Value) : RkpValue;

/// <summary>
/// A boolean value from the .rkp file (<c>true</c> or <c>false</c>).
/// </summary>
/// <param name="Value">The boolean value.</param>
public sealed record RkpBoolean(bool Value) : RkpValue;

/// <summary>
/// A table (associative or sequential) from the .rkp file, delimited by <c>{ }</c>.
/// Tables can contain a mix of named entries (<c>key = value</c>) and anonymous entries (bare values).
/// </summary>
/// <param name="Entries">The ordered list of table entries.</param>
public sealed record RkpTable(List<RkpTableEntry> Entries) : RkpValue
{
    /// <summary>
    /// Looks up a named entry by key. Returns null if the entry does not exist.
    /// </summary>
    /// <param name="name">The entry name (case-sensitive).</param>
    /// <returns>The value for the entry, or null if not found.</returns>
    public RkpValue? Get(string name)
    {
        for (int i = 0; i < Entries.Count; i++)
        {
            if (Entries[i].Name == name)
                return Entries[i].Value;
        }
        return null;
    }

    /// <summary>
    /// Gets a named string entry, or null if missing or not a string.
    /// </summary>
    public string? GetString(string name) => (Get(name) as RkpString)?.Value;

    /// <summary>
    /// Gets a named integer entry, or null if missing or not an integer.
    /// </summary>
    public int? GetInt(string name) => (Get(name) as RkpInteger)?.Value;

    /// <summary>
    /// Gets a named boolean entry, or null if missing or not a boolean.
    /// </summary>
    public bool? GetBool(string name) => (Get(name) as RkpBoolean)?.Value;

    /// <summary>
    /// Gets a named table entry, or null if missing or not a table.
    /// </summary>
    public RkpTable? GetTable(string name) => Get(name) as RkpTable;

    /// <summary>
    /// Returns all anonymous (unnamed) entries whose values are tables.
    /// </summary>
    public IEnumerable<RkpTable> AnonymousTables()
    {
        for (int i = 0; i < Entries.Count; i++)
        {
            if (Entries[i].Name is null && Entries[i].Value is RkpTable table)
                yield return table;
        }
    }

    /// <summary>
    /// Returns all named entries as key-value pairs.
    /// </summary>
    public IEnumerable<(string Name, RkpValue Value)> NamedEntries()
    {
        for (int i = 0; i < Entries.Count; i++)
        {
            if (Entries[i].Name is not null)
                yield return (Entries[i].Name!, Entries[i].Value);
        }
    }
}

/// <summary>
/// A single entry in an <see cref="RkpTable"/>. Named entries have a non-null <see cref="Name"/>;
/// anonymous entries (array-like items) have a null <see cref="Name"/>.
/// </summary>
/// <param name="Name">The field name, or null for anonymous (positional) entries.</param>
/// <param name="Value">The entry value.</param>
public sealed record RkpTableEntry(string? Name, RkpValue Value);

#endregion

/// <summary>
/// A recursive-descent parser for the .rkp (Relic Key-binding Profile) file format.
/// </summary>
/// <remarks>
/// <para>
/// This parser consumes the token stream produced by <see cref="RkpTokenizer"/> and builds
/// an intermediate AST made up of <see cref="RkpValue"/> nodes. The root of a valid .rkp
/// document is a single top-level assignment (e.g., <c>profile = { ... }</c>) whose value
/// is an <see cref="RkpTable"/>.
/// </para>
/// <para>
/// Grammar handled:
/// <code>
/// document    := assignment EOF
/// assignment  := IDENTIFIER '=' value
/// value       := STRING | INTEGER | BOOLEAN | table
/// table       := '{' (table_entry (',' table_entry)* ','?)? '}'
/// table_entry := IDENTIFIER '=' value | value
/// </code>
/// </para>
/// </remarks>
public sealed class RkpParser
{
    private readonly List<RkpToken> _tokens;
    private int _position;

    /// <summary>
    /// Initializes a new <see cref="RkpParser"/> with the given token list.
    /// </summary>
    /// <param name="tokens">
    /// The token list produced by <see cref="RkpTokenizer.TokenizeAll"/>.
    /// Must end with an <see cref="RkpTokenType.EOF"/> token.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="tokens"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="tokens"/> is empty.</exception>
    private RkpParser(List<RkpToken> tokens)
    {
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        if (_tokens.Count == 0)
            throw new ArgumentException("Token list must not be empty.", nameof(tokens));
        _position = 0;
    }

    /// <summary>
    /// Parses an .rkp file from its source text and returns the root table.
    /// </summary>
    /// <param name="source">The full text content of an .rkp file.</param>
    /// <returns>
    /// An <see cref="RkpTable"/> representing the top-level value
    /// (i.e., the right-hand side of <c>profile = { ... }</c>).
    /// </returns>
    /// <exception cref="RkpParseException">The input is not valid .rkp syntax.</exception>
    public static RkpTable Parse(string source)
    {
        var tokenizer = new RkpTokenizer(source);
        var tokens = tokenizer.TokenizeAll();
        var parser = new RkpParser(tokens);
        return parser.ParseDocument();
    }

    /// <summary>
    /// Parses an .rkp file from a pre-tokenized token list and returns the root table.
    /// </summary>
    /// <param name="tokens">The token list (must end with EOF).</param>
    /// <returns>The root <see cref="RkpTable"/>.</returns>
    /// <exception cref="RkpParseException">The tokens do not form valid .rkp syntax.</exception>
    public static RkpTable Parse(List<RkpToken> tokens)
    {
        var parser = new RkpParser(tokens);
        return parser.ParseDocument();
    }

    /// <summary>
    /// Parses the top-level document: a single assignment whose value must be a table.
    /// </summary>
    private RkpTable ParseDocument()
    {
        // Expect: IDENTIFIER '=' table
        var (name, value) = ParseAssignment();

        // Consume optional trailing comma after the root assignment.
        if (Current.Type == RkpTokenType.Comma)
            Advance();

        if (Current.Type != RkpTokenType.EOF)
            throw new RkpParseException(
                $"Expected end of file after top-level assignment, found {Current.Type} '{Current.Value}'",
                Current.Line);

        if (value is not RkpTable rootTable)
            throw new RkpParseException(
                $"Top-level assignment '{name}' must be a table, but got {value.GetType().Name}",
                _tokens[0].Line);

        return rootTable;
    }

    /// <summary>
    /// Parses a named assignment: <c>IDENTIFIER '=' value</c>.
    /// </summary>
    /// <returns>A tuple of the identifier name and the parsed value.</returns>
    private (string Name, RkpValue Value) ParseAssignment()
    {
        var identToken = Expect(RkpTokenType.Identifier, "identifier");
        Expect(RkpTokenType.Equals, "'='");
        var value = ParseValue();
        return (identToken.Value, value);
    }

    /// <summary>
    /// Parses any value: string, integer, boolean, or table.
    /// </summary>
    private RkpValue ParseValue()
    {
        var token = Current;

        return token.Type switch
        {
            RkpTokenType.StringLiteral => ParseStringLiteral(),
            RkpTokenType.IntegerLiteral => ParseIntegerLiteral(),
            RkpTokenType.BooleanLiteral => ParseBooleanLiteral(),
            RkpTokenType.OpenBrace => ParseTable(),
            _ => throw new RkpParseException(
                $"Expected a value (string, integer, boolean, or table), found {token.Type} '{token.Value}'",
                token.Line)
        };
    }

    /// <summary>
    /// Parses a string literal token into an <see cref="RkpString"/>.
    /// </summary>
    private RkpString ParseStringLiteral()
    {
        var token = Advance();
        return new RkpString(token.Value);
    }

    /// <summary>
    /// Parses an integer literal token into an <see cref="RkpInteger"/>.
    /// </summary>
    private RkpInteger ParseIntegerLiteral()
    {
        var token = Advance();
        if (!int.TryParse(token.Value, out var intValue))
            throw new RkpParseException($"Invalid integer literal '{token.Value}'", token.Line);
        return new RkpInteger(intValue);
    }

    /// <summary>
    /// Parses a boolean literal token into an <see cref="RkpBoolean"/>.
    /// </summary>
    private RkpBoolean ParseBooleanLiteral()
    {
        var token = Advance();
        return new RkpBoolean(token.Value == "true");
    }

    /// <summary>
    /// Parses a table: <c>'{' (table_entry (',' table_entry)* ','?)? '}'</c>.
    /// Table entries can be named (<c>IDENTIFIER '=' value</c>) or anonymous (bare values/tables).
    /// </summary>
    private RkpTable ParseTable()
    {
        var openBrace = Expect(RkpTokenType.OpenBrace, "'{'");
        var entries = new List<RkpTableEntry>();

        while (Current.Type != RkpTokenType.CloseBrace)
        {
            if (Current.Type == RkpTokenType.EOF)
                throw new RkpParseException(
                    $"Unterminated table (opened at line {openBrace.Line})",
                    Current.Line);

            var entry = ParseTableEntry();
            entries.Add(entry);

            // Consume optional comma between entries.
            if (Current.Type == RkpTokenType.Comma)
                Advance();
        }

        Expect(RkpTokenType.CloseBrace, "'}'");
        return new RkpTable(entries);
    }

    /// <summary>
    /// Parses a single table entry. Determines whether it is a named assignment
    /// or an anonymous value by looking ahead.
    /// </summary>
    private RkpTableEntry ParseTableEntry()
    {
        // Check if this is a named assignment: IDENTIFIER '=' ...
        if (Current.Type == RkpTokenType.Identifier && Peek.Type == RkpTokenType.Equals)
        {
            var (name, value) = ParseAssignment();
            return new RkpTableEntry(name, value);
        }

        // Otherwise it is an anonymous entry (e.g., a bare table { ... }).
        var anonValue = ParseValue();
        return new RkpTableEntry(null, anonValue);
    }

    #region Token navigation helpers

    /// <summary>
    /// Gets the current token without advancing.
    /// </summary>
    private RkpToken Current => _position < _tokens.Count
        ? _tokens[_position]
        : _tokens[^1]; // EOF sentinel

    /// <summary>
    /// Gets the next token (look-ahead by 1) without advancing.
    /// </summary>
    private RkpToken Peek => _position + 1 < _tokens.Count
        ? _tokens[_position + 1]
        : _tokens[^1]; // EOF sentinel

    /// <summary>
    /// Advances past the current token and returns it.
    /// </summary>
    private RkpToken Advance()
    {
        var token = Current;
        if (_position < _tokens.Count)
            _position++;
        return token;
    }

    /// <summary>
    /// Asserts that the current token has the expected type, consumes it, and returns it.
    /// Throws <see cref="RkpParseException"/> if the token type does not match.
    /// </summary>
    /// <param name="expectedType">The expected token type.</param>
    /// <param name="description">A human-readable description for error messages (e.g., "'{'").</param>
    private RkpToken Expect(RkpTokenType expectedType, string description)
    {
        var token = Current;
        if (token.Type != expectedType)
            throw new RkpParseException(
                $"Expected {description}, found {token.Type} '{token.Value}'",
                token.Line);
        return Advance();
    }

    #endregion
}
