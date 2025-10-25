using OLangCompiler.Parser.Nodes;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser;

public interface IParser
{
    public ProgramNode ParseProgram(List<BaseToken> tokens, ErrorHelper errorHelper);
}