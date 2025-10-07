using OLangCompiler.Parser.Nodes;
using OLangCompiler.Tokens;

namespace OLangCompiler;

public static class ErrorHelper
{
    
    public static Exception UnknownVariant(string name, Type type)
    {
        return new Exception($"Unknown {name} type: {type}");
    }

    public static Exception ExpectedToken<T>(IToken token) where T : IToken
    {
        return ShowErrorMessageAtToken($"Expected {NameOfTokenType<T>()}", token);
    }
    
    public static Exception ExpectedValue(string expectedName, IToken token)
    {
        return ShowErrorMessageAtToken($"Expected {expectedName}", token);
    }

    public static Exception UnexpectedEndOfInput(IToken token)
    {
        return ShowErrorMessageAtToken($"Unexpected end of input after {token.GetType().Name}", token);
    }

    public static Exception ShowErrorMessageAtToken(string message, IToken token)
    {
        return new Exception(message);
    }
    
    public static Exception ShowErrorMessageAtNode(string message, INode node)
    {
        return new Exception(message);
    }

    private static string NameOfTokenType<T>() where T : IToken
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

        throw UnknownVariant("token type", typeof(T));
    }
}