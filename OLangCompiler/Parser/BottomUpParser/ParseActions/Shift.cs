namespace OLangCompiler.Parser.BottomUpParser.ParseActions;

public class Shift(int newState) : IParseAction
{
    public int NewState = newState;
}