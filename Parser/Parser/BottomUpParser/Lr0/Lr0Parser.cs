using ErrorHelper;
using Lexing;

namespace Parser.Parser.BottomUpParser.Lr0;

public class Lr0Parser : IParser
{
    public INode ParseProgram(IGrammar grammar, List<BaseToken> tokens, IErrorHelper errorHelper)
    {
        return new LrParser().ParseProgram(new Lr0ParseTable(grammar, errorHelper), tokens, errorHelper);
    }
}