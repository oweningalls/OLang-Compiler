using Lexing;

namespace ErrorHelper;

public interface IErrorHelper
{
    public Exception UnknownVariant(string name, Type type);

    public Exception ShowErrorMessage(string message, SourceSpan token);

    public Exception ShowErrorMessageAtElement(string message, IGrammarElement element);

    public Exception UnexpectedChar(char c);

    public Exception UnexpectedEndOfInputAfterChar(char c);
}