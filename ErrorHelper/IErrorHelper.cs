using Lexing;

namespace ErrorHelper;

public interface IErrorHelper
{
    public Exception UnknownVariant(string name, Type type);

    public Exception ExpectedToken<T>(BaseToken token) where T : BaseToken;

    public Exception ExpectedValue(string expectedName, BaseToken token);

    public Exception ShowErrorMessageAtToken(string message, BaseToken token);

    public Exception ShowErrorMessageAtElement(string message, IGrammarElement element);

    public Exception ShowErrorMessageAtNode(string message, INode node);

    public Exception UnexpectedChar(char c);

    public Exception UnexpectedEndOfInputAfterChar(char c);
}