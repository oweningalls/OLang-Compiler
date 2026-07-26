using OLangCompiler.Parser.BottomUpParser;
using OLangCompiler.Parser.ParseTree;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser;

public interface IParser
{
    public INode ParseProgram(IGrammar grammar, List<BaseToken> tokens, IErrorHelper errorHelper);
}