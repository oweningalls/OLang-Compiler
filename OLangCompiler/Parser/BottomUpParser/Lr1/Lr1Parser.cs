using OLangCompiler.Parser.ParseTree;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.BottomUpParser.Lr1;

public class Lr1Parser : IParser
{
    public INode ParseProgram(IGrammar grammar, List<BaseToken> tokens, IErrorHelper errorHelper)
    {
        return new LrParser().ParseProgram(new Lr1ParseTable(grammar, errorHelper), tokens, errorHelper);
    }
}