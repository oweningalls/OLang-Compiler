namespace Lexing;

public abstract class BaseToken : IGrammarElement
{
    public SourceSpan Span { get; set; }
}