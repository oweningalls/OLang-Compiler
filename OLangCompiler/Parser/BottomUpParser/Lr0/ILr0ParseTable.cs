using OLangCompiler.Parser.BottomUpParser.ParseActions;

namespace OLangCompiler.Parser.BottomUpParser.Lr0;

public interface ILr0ParseTable
{
    public IParseAction GetActionAndTransition(IGrammarElement next);
}