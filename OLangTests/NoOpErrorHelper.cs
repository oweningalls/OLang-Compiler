using ErrorHelper;
using Lexing;

namespace OLangTests;

public class NoOpErrorHelper : IErrorHelper
{
    public Exception UnknownVariant(string name, Type type)
    {
        return null;
    }

    public Exception ShowErrorMessage(string message, SourceSpan token)
    {
        return null;
    }

    public Exception ShowErrorMessageAtElement(string message, IGrammarElement element)
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