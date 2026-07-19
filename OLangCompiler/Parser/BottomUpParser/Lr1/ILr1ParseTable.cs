using OLangCompiler.Parser.BottomUpParser.ParseActions;

namespace OLangCompiler.Parser.BottomUpParser.Lr1;

public interface ILr1ParseTable
{
    public IParseAction GetActionAndState(IGrammarElement next, int state);
}