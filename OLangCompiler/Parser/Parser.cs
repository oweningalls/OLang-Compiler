using OLangCompiler.Parser.Nodes;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser;

public class Parser
{
    public ProgramNode ParseProgram(List<IToken> tokens)
    {
        _tokens = tokens;
        _currentIndex = 0;

        TryConsume<ExitToken>("Expected exit");
        var intLit = TryConsume<IntLiteralToken>("Expected int literal");
        TryConsume<SemicolonToken>("Expected semicolon");
        if (Peek() is { } token)
        {
            throw new Exception($"Expected end of input but was {token}");
        }

        return new ProgramNode(new Term(intLit.Value));
    }
    

    private List<IToken>? _tokens;
    private int _currentIndex;
    private IToken? Peek()
    {
        if (_currentIndex >= _tokens!.Count) return null;

        return _tokens[_currentIndex];
    }

    private IToken Consume()
    {
        var ret = Peek();
        _currentIndex++;
        if (ret == null)
        {
            throw new Exception("Unexpected end of token stream");
        }

        return ret;
    }

    private T TryConsume<T>(string errorMessage) where T : class, IToken
    {
        return Consume() as T ?? throw new Exception(errorMessage);
    }
}