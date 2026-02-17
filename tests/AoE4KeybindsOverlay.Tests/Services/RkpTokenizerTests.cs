using AoE4KeybindsOverlay.Services.Keybinding;

namespace AoE4KeybindsOverlay.Tests.Services;

public class RkpTokenizerTests
{
    [Fact]
    public void Tokenize_SimpleStringAssignment_ProducesCorrectTokens()
    {
        var tokenizer = new RkpTokenizer("name = \"test\"");
        var tokens = tokenizer.TokenizeAll();

        // Expect: Identifier("name"), Equals, StringLiteral("test"), EOF
        Assert.Equal(4, tokens.Count);
        Assert.Equal(RkpTokenType.Identifier, tokens[0].Type);
        Assert.Equal("name", tokens[0].Value);
        Assert.Equal(RkpTokenType.Equals, tokens[1].Type);
        Assert.Equal(RkpTokenType.StringLiteral, tokens[2].Type);
        Assert.Equal("test", tokens[2].Value);
        Assert.Equal(RkpTokenType.EOF, tokens[3].Type);
    }

    [Fact]
    public void Tokenize_NegativeInteger_ProducesCorrectTokens()
    {
        var tokenizer = new RkpTokenizer("repeatCount = -1");
        var tokens = tokenizer.TokenizeAll();

        // Expect: Identifier("repeatCount"), Equals, IntegerLiteral("-1"), EOF
        Assert.Equal(4, tokens.Count);
        Assert.Equal(RkpTokenType.Identifier, tokens[0].Type);
        Assert.Equal("repeatCount", tokens[0].Value);
        Assert.Equal(RkpTokenType.Equals, tokens[1].Type);
        Assert.Equal(RkpTokenType.IntegerLiteral, tokens[2].Type);
        Assert.Equal("-1", tokens[2].Value);
        Assert.Equal(RkpTokenType.EOF, tokens[3].Type);
    }

    [Fact]
    public void Tokenize_Boolean_ProducesCorrectTokens()
    {
        var tokenizer = new RkpTokenizer("warnConflicts = true");
        var tokens = tokenizer.TokenizeAll();

        // Expect: Identifier("warnConflicts"), Equals, BooleanLiteral("true"), EOF
        Assert.Equal(4, tokens.Count);
        Assert.Equal(RkpTokenType.Identifier, tokens[0].Type);
        Assert.Equal("warnConflicts", tokens[0].Value);
        Assert.Equal(RkpTokenType.Equals, tokens[1].Type);
        Assert.Equal(RkpTokenType.BooleanLiteral, tokens[2].Type);
        Assert.Equal("true", tokens[2].Value);
        Assert.Equal(RkpTokenType.EOF, tokens[3].Type);
    }

    [Fact]
    public void Tokenize_NestedBraces_ProducesCorrectTokens()
    {
        var tokenizer = new RkpTokenizer("{ { } }");
        var tokens = tokenizer.TokenizeAll();

        // Expect: OpenBrace, OpenBrace, CloseBrace, CloseBrace, EOF
        Assert.Equal(5, tokens.Count);
        Assert.Equal(RkpTokenType.OpenBrace, tokens[0].Type);
        Assert.Equal(RkpTokenType.OpenBrace, tokens[1].Type);
        Assert.Equal(RkpTokenType.CloseBrace, tokens[2].Type);
        Assert.Equal(RkpTokenType.CloseBrace, tokens[3].Type);
        Assert.Equal(RkpTokenType.EOF, tokens[4].Type);
    }

    [Fact]
    public void Tokenize_MinimalRkpSnippet_ProducesExpectedTokenStream()
    {
        const string source = @"profile = {
    bindingGroups = {
        camera = {
            {
                command = ""zoom_in"",
                keycombos = {
                    {
                        combo = ""MouseWheelUp"",
                        repeatCount = -1,
                    },
                },
            },
        },
    },
    name = ""test"",
    warnConflicts = true,
}";

        var tokenizer = new RkpTokenizer(source);
        var tokens = tokenizer.TokenizeAll();

        // The token stream should end with EOF
        Assert.Equal(RkpTokenType.EOF, tokens[^1].Type);

        // Verify that key identifiers, strings, and structural tokens are present
        Assert.Contains(tokens, t => t.Type == RkpTokenType.Identifier && t.Value == "profile");
        Assert.Contains(tokens, t => t.Type == RkpTokenType.Identifier && t.Value == "bindingGroups");
        Assert.Contains(tokens, t => t.Type == RkpTokenType.Identifier && t.Value == "camera");
        Assert.Contains(tokens, t => t.Type == RkpTokenType.Identifier && t.Value == "command");
        Assert.Contains(tokens, t => t.Type == RkpTokenType.StringLiteral && t.Value == "zoom_in");
        Assert.Contains(tokens, t => t.Type == RkpTokenType.StringLiteral && t.Value == "MouseWheelUp");
        Assert.Contains(tokens, t => t.Type == RkpTokenType.StringLiteral && t.Value == "test");
        Assert.Contains(tokens, t => t.Type == RkpTokenType.BooleanLiteral && t.Value == "true");
        Assert.Contains(tokens, t => t.Type == RkpTokenType.IntegerLiteral && t.Value == "-1");
    }

    [Fact]
    public void Tokenize_CommaToken_IsProduced()
    {
        var tokenizer = new RkpTokenizer("a = \"b\", c = \"d\"");
        var tokens = tokenizer.TokenizeAll();

        Assert.Contains(tokens, t => t.Type == RkpTokenType.Comma);
    }

    [Fact]
    public void Tokenize_EmptyString_ProducesOnlyEOF()
    {
        var tokenizer = new RkpTokenizer("");
        var tokens = tokenizer.TokenizeAll();

        Assert.Single(tokens);
        Assert.Equal(RkpTokenType.EOF, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_TracksLineNumbers()
    {
        var tokenizer = new RkpTokenizer("a\nb\nc");
        var tokens = tokenizer.TokenizeAll();

        Assert.Equal(1, tokens[0].Line); // "a" on line 1
        Assert.Equal(2, tokens[1].Line); // "b" on line 2
        Assert.Equal(3, tokens[2].Line); // "c" on line 3
    }
}
