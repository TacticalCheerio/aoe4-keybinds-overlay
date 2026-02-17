namespace AoE4KeybindsOverlay.Services.Keybinding;

/// <summary>
/// Exception thrown when parsing of an .rkp file fails due to syntax or structural errors.
/// </summary>
public sealed class RkpParseException : Exception
{
    /// <summary>
    /// Gets the 1-based line number where the parse error occurred, or -1 if unknown.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Initializes a new <see cref="RkpParseException"/> with a message and line number.
    /// </summary>
    /// <param name="message">A description of the parse error.</param>
    /// <param name="line">The 1-based line number where the error occurred.</param>
    public RkpParseException(string message, int line)
        : base($"Line {line}: {message}")
    {
        Line = line;
    }

    /// <summary>
    /// Initializes a new <see cref="RkpParseException"/> with a message.
    /// </summary>
    /// <param name="message">A description of the parse error.</param>
    public RkpParseException(string message)
        : base(message)
    {
        Line = -1;
    }

    /// <summary>
    /// Initializes a new <see cref="RkpParseException"/> with a message and inner exception.
    /// </summary>
    public RkpParseException(string message, Exception innerException)
        : base(message, innerException)
    {
        Line = -1;
    }
}
