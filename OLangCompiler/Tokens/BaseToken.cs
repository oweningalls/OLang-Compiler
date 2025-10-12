namespace OLangCompiler.Tokens;

public abstract class BaseToken
{
    public int LineNumber;
    public int RelativeStartCharNumber;
    public int RelativeEndCharNumber;
    public int AbsoluteStartCharNumber;
    public int AbsoluteEndCharNumber;
}