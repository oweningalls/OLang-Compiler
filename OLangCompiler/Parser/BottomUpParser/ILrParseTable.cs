using OLangCompiler.Parser.BottomUpParser.ParseActions;

namespace OLangCompiler.Parser.BottomUpParser;

public interface ILrParseTable
{
    public IParseAction GetActionAndState(IGrammarElement? next, int state);
}