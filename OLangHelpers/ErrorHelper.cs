using ErrorHelper;
using Lexing;
using OLangLexing;
using OLangTokens.Tokens;

namespace OLangHelpers;

public class ErrorHelper(SourceReader source) : IErrorHelper
{
    private SourceReader _source = source;
    
    public Exception UnknownVariant(string name, Type type)
    {
        return new Exception($"Unknown {name} type: {type}");
    }

    public Exception ExpectedToken<T>(BaseToken token) where T : BaseToken
    {
        return ShowErrorMessageAtToken($"Expected {NameOfTokenType<T>()}.", token);
    }
    
    public Exception ExpectedValue(string expectedName, BaseToken token)
    {
        return ShowErrorMessageAtToken($"Expected {expectedName}", token);
    }

    public Exception ShowErrorMessageAtToken(string message, BaseToken token)
    {
        var quotedCode = GetInputLine(token);
        var (line, relativeCharacterNumber) = _source.GetLineAndRelativeCharacterNumber(token.Span);
        
        var indicator = GetIndicator(relativeCharacterNumber + 1, relativeCharacterNumber + token.Span.Length + 1); // this will break if a token spans a newline
        var errorMessage = $"{message} on line {line + 1}, character {relativeCharacterNumber}\n`{quotedCode}`\n{indicator}";
        return new Exception(errorMessage);
    }

    private string GetIndicator(int pointerStart, int pointerEnd)
    {
        return $"{new string(' ', pointerStart)}{new string('^', pointerEnd - pointerStart + 1)}";
    }

    public Exception ShowErrorMessageAtElement(string message, IGrammarElement element)
    {
        return element switch
        {
            INode node => ShowErrorMessageAtNode(message, node),
            BaseToken token => ShowErrorMessageAtToken(message, token),
            _ => throw UnknownVariant("grammar element", element.GetType())
        };
    }
    
    public Exception ShowErrorMessageAtNode(string message, INode node)
    {
        return new Exception(message);
    }

    public Exception UnexpectedChar(char c)
    {
        return new Exception($"Unexpected character: `{c}`");
    }

    public Exception UnexpectedEndOfInputAfterChar(char c)
    {
        return new Exception($"Unexpected end of input after `{c}`");
    }

    private string GetInputLine(BaseToken token)
    {
        var (line, _) = _source.GetLineAndRelativeCharacterNumber(token.Span);
        return _source.GetLine(line);
    }

    private string NameOfTokenType<T>() where T : BaseToken
    {
        if (typeof(T) == typeof(BoolLiteralToken)) return "boolean literal";
        if (typeof(T) == typeof(BoolTypeToken)) return "'bool'";
        if (typeof(T) == typeof(EqualsToken)) return "'='";
        if (typeof(T) == typeof(ExitToken)) return "'exit'";
        if (typeof(T) == typeof(IdentifierToken)) return "identifier";
        if (typeof(T) == typeof(IntLiteralToken)) return "int literal";
        if (typeof(T) == typeof(IntTypeToken)) return "'int'";
        if (typeof(T) == typeof(LeftParenToken)) return "'('";
        if (typeof(T) == typeof(RightParenToken)) return "')'";
        if (typeof(T) == typeof(LetToken)) return "'let'";
        if (typeof(T) == typeof(MinusToken)) return "'-'";
        if (typeof(T) == typeof(PlusToken)) return "'+'";
        if (typeof(T) == typeof(SemicolonToken)) return "';'";
        if (typeof(T) == typeof(LeftCurlyToken)) return "'{'";
        if (typeof(T) == typeof(RightCurlyToken)) return "'}'";
        if (typeof(T) == typeof(BaseToken)) return "set operator";

        throw UnknownVariant("token", typeof(T));
    }
}