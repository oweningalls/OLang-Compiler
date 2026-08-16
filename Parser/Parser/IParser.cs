using ErrorHelper;
using Lexing;

namespace Parser.Parser;

public interface IParser
{
    public INode ParseProgram(IGrammar grammar, List<BaseToken> tokens, IErrorHelper errorHelper);
}