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

            if (Peek() == '=')
            {
                _ = Consume();
                if (Peek() == '=')
                {
                    _ = Consume();
                    tokens.Add(new DoubleEqualsToken());
                    continue;
                }
                
                tokens.Add(new EqualsToken());
                continue;
            }

            if (Peek() == '!')
            {
                _ = Consume();
                if (Peek() == '=')
                {
                    _ = Consume();
                    tokens.Add(new NotEqualToken());
                    continue;
                }

                tokens.Add(new NotToken());
                continue;
            }

            if (TryParseEqualsToken() is { } token)
            {
                tokens.Add(token);
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

    private IToken? TryParseEqualsToken()
    {
        if (EqualsOperatorMap.ContainsKey(Peek()!.Value))
        {
            var funcs = EqualsOperatorMap[Consume()];
            if (Peek() is '=')
            {
                _ = Consume();
                return funcs.Item2.Invoke();
            }

            return funcs.Item1.Invoke();
        }

        return null;
    }
    
    // operators that could be x, or could be x= (e.g. ! and !=)
    // first function makes x token, second makes the x= token
    private static readonly Dictionary<char, (Func<IToken>, Func<IToken>)> EqualsOperatorMap = new()
    {
        { '=',  (() => new EqualsToken(), () => new DoubleEqualsToken())},
        { '+',  (() => new PlusToken(), () => new PlusEqualsToken())},
        { '-',  (() => new MinusToken(), () => new MinusEqualsToken())},
    };
    

    private IToken TokenizeLetter()
    {
        var buffer = "";
        while (Peek() is {} c && (char.IsLetterOrDigit(c) || c == '_'))
        {
            buffer += Consume();
        }

        if (KeywordMap.TryGetValue(buffer, out var tokenFunc))
        {
            return tokenFunc.Invoke();
        }

        return new IdentifierToken(buffer);
    }

    private static readonly Dictionary<string, Func<IToken>> KeywordMap = new()
    {
        { "exit",  () => new ExitToken()},
        { "let", () => new LetToken() },
        { "int", () => new IntTypeToken() },
        { "bool", () => new BoolTypeToken() },
        { "true", () => new BoolLiteralToken(true) },
        { "false", () => new BoolLiteralToken(false) },
        { "if", () => new IfToken() },
        { "while", () => new WhileToken() }
    };

    private IToken? TryParseOperator()
    {
        if (OperatorMap.ContainsKey(Peek()!.Value))
        {
            return OperatorMap[Consume()].Invoke();
        }

        return null;
    }

    private static readonly Dictionary<char, Func<IToken>> OperatorMap = new()
    {
        { '=',  () => new EqualsToken()},
        { ';', () => new SemicolonToken() },
        { '+', () => new PlusToken() },
        {'(', () => new LeftParenToken() },
        {')', () => new RightParenToken() },
        {'{', () => new LeftCurlyToken() },
        {'}', () => new RightCurlyToken() }
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