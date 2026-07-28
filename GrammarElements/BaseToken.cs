namespace Lexing;

public abstract class BaseToken : IGrammarElement
{
    public int LineNumber;
    public int RelativeStartCharNumber;
    public int RelativeEndCharNumber;
    public int AbsoluteStartCharNumber;
    public int AbsoluteEndCharNumber;
}