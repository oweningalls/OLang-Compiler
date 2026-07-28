using ErrorHelper;
using Lexing;
using OLangCompiler.Parser.BottomUpParser;

namespace OLangCompiler.Parser;

public interface IParser
{
    public INode ParseProgram(IGrammar grammar, List<BaseToken> tokens, IErrorHelper errorHelper);
}