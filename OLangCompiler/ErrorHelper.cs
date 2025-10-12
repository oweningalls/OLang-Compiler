using OLangCompiler.Parser.Nodes;
using OLangCompiler.Tokens;

namespace OLangCompiler;

public class ErrorHelper(string input)
{
    private string _input = input;
    private string[] _inputLines = input.Split('\n');
    
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

    public Exception UnexpectedEndOfInput(BaseToken token)
    {
        return ShowErrorMessageAtToken($"Unexpected end of input after `{GetContainedValue(token)}`", token);
    }

    public Exception ShowErrorMessageAtToken(string message, BaseToken token)
    {
        var quotedCode = GetInputLine(token);
        var indicator = GetIndicator(token.RelativeStartCharNumber + 1, token.RelativeEndCharNumber + 1);
        var errorMessage = $"{message} on line {token.LineNumber}, character {token.RelativeStartCharNumber}\n`{quotedCode}`\n{indicator}";
        return new Exception(errorMessage);
    }

    private string GetContainedValue(BaseToken token)
    {
        return _input.Substring(token.AbsoluteStartCharNumber, token.AbsoluteEndCharNumber - token.AbsoluteStartCharNumber + 1);
    }

    private string GetIndicator(int pointerStart, int pointerEnd)
    {
        return $"{new string(' ', pointerStart)}{new string('^', pointerEnd - pointerStart + 1)}";
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
        return _inputLines[token.LineNumber - 1];
        // return _input.Substring(token.AbsoluteStartCharNumber, token.AbsoluteEndCharNumber - token.AbsoluteStartCharNumber + 1);
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
        if (typeof(T) == typeof(BaseAssignmentOperatorToken)) return "set operator";

        throw UnknownVariant("token", typeof(T));
    }
}