namespace OLangCompiler.Tokens;

public class Tokenizer
{
    public List<IToken> Tokenize(string input)
    {
        _input = input;
        _currentIndex = 0;
        var tokens = new List<IToken>();
        while (Peek() != null)
        {
            if (char.IsLetter(Peek()!.Value))
            {
                tokens.Add(TokenizeLetter());
                continue;
            }

            if (char.IsDigit(Peek()!.Value))
            {
                var buffer = "";
                buffer += Consume();
                while (Peek() is { } c && char.IsDigit(c))
                {
                    buffer += Consume();
                }
                
                tokens.Add(new IntLiteralToken(int.Parse(buffer)));
                continue;
            }

            if (TryParseOperator() is { } op)
            {
                tokens.Add(op);
                continue;
            }

            if (char.IsWhiteSpace(Peek()!.Value))
            {
                _ = Consume();
                continue;
            }

            throw new Exception($"Unexpected character: `{Peek()}`");
        }

        return tokens;
    }

    private IToken TokenizeLetter()
    {
        var buffer = "";
        while (Peek() is {} c && char.IsLetterOrDigit(c))
        {
            buffer += Consume();
        }

        if (_keywordMap.TryGetValue(buffer, out var tokenFunc))
        {
            return tokenFunc.Invoke();
        }

        return new IdentifierToken(buffer);
    }

    private static readonly Dictionary<string, Func<IToken>> _keywordMap = new()
    {
        { "exit",  () => new ExitToken()},
        { "let", () => new LetToken() }
    };

    private IToken? TryParseOperator()
    {
        if (_operatormap.ContainsKey(Peek()!.Value))
        {
            return _operatormap[Consume()].Invoke();
        }

        return null;
    }

    private static readonly Dictionary<char, Func<IToken>> _operatormap = new()
    {
        { '=',  () => new EqualsToken()},
        { ';', () => new SemicolonToken() },
        { '+', () => new PlusToken() },
        {'(', () => new LeftParenToken() },
        {')', () => new RightParenToken() }
    };
    
    private string? _input;
    private int _currentIndex;

    private char? Peek()
    {
        if (_currentIndex >= _input!.Length) return null;

        return _input[_currentIndex];
    }

    private char Consume()
    {
        var ret = Peek();
        _currentIndex++;
        if (ret == null)
        {
            throw new Exception("Unexpected end of input");
        }

        return (char)ret;
    }
}