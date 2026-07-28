using ErrorHelper;
using Lexing;

namespace OLangTests;

public class NoOpErrorHelper : IErrorHelper
{
    public Exception UnknownVariant(string name, Type type)
    {
        return null;
    }

    public Exception ExpectedToken<T>(BaseToken token) where T : BaseToken
    {
        return null;
    }

    public Exception ExpectedValue(string expectedName, BaseToken token)
    {
        return null;
    }

    public Exception ShowErrorMessageAtToken(string message, BaseToken token)
    {
        return null;
    }

    public Exception ShowErrorMessageAtElement(string message, IGrammarElement element)
    {
        return null;
    }

    public Exception ShowErrorMessageAtNode(string message, INode node)
    {
        return null;
    }

    public Exception UnexpectedChar(char c)
    {
        return null;
    }

    public Exception UnexpectedEndOfInputAfterChar(char c)
    {
        return null;
    }
}