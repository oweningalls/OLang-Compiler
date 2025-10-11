namespace OLangCompiler.Tokens;

public class Tokenizer
{
    public List<IToken> Tokenize(string input)
    {
        _input = input;
        _currentIndex = 0;
        var tokens = new List<IToken>();
        var token = GetNextToken();
        while (token != null)
        {
            tokens.Add(token);
            token = GetNextToken();
        }

        return tokens;
    }

    private IToken? GetNextToken()
    {
        if (Peek() == null)
        {
            return null;
        }
        
        if (char.IsLetter(Peek()!.Value))
        {
            return TokenizeLetter();
            
        }

        if (char.IsDigit(Peek()!.Value))
        {
            var buffer = "";
            buffer += Consume();
            while (Peek() is { } c && char.IsDigit(c))
            {
                buffer += Consume();
            }

            return new IntLiteralToken(int.Parse(buffer));
        }

        if (TryParseEqualsToken() is { } token)
        {
            return token;
        }

        if (Peek() == '.')
        {
            _ = Consume();
            if (Peek() == '.')
            {
                _ = Consume();
                return new RangeToken();
            }

            if (Peek() is null)
            {
                throw ErrorHelper.UnexpectedEndOfInputAfterChar(Peek(-1)!.Value);
            }

            throw ErrorHelper.UnexpectedChar(Peek()!.Value);
        }

        if (TryParseOperator() is { } op)
        {
            return op;
        }

        if (char.IsWhiteSpace(Peek()!.Value))
        {
            _ = Consume();
            return GetNextToken();
        }

        throw new Exception($"Unexpected character: `{Peek()}`");
    }

    private IToken? TryParseEqualsToken()
    {
        if (!EqualsOperatorMap.ContainsKey(Peek()!.Value)) return null;
        var funcs = EqualsOperatorMap[Consume()];
        if (Peek() is '=')
        {
            _ = Consume();
            return funcs.Item2.Invoke();
        }

        return funcs.Item1.Invoke();
    }

    // operators that could be x, or could be x= (e.g. ! and !=)
    // first function makes x token, second makes the x= token
    private static readonly Dictionary<char, (Func<IToken>, Func<IToken>)> EqualsOperatorMap = new()
    {
        { '=', (() => new EqualsToken(), () => new DoubleEqualsToken()) },
        { '+', (() => new PlusToken(), () => new PlusEqualsToken()) },
        { '-', (() => new MinusToken(), () => new MinusEqualsToken()) },
        { '!', (() => new NotToken(), () => new NotEqualToken()) },
        { '>', (() => new GreaterToken(), () => new GreaterOrEqualToken()) },
        { '<', (() => new LessToken(), () => new LessOrEqualToken()) }
    };


    private IToken TokenizeLetter()
    {
        var buffer = "";
        while (Peek() is { } c && (char.IsLetterOrDigit(c) || c == '_'))
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
        { "exit", () => new ExitToken() },
        { "let", () => new LetToken() },
        { "int", () => new IntTypeToken() },
        { "bool", () => new BoolTypeToken() },
        { "true", () => new BoolLiteralToken(true) },
        { "false", () => new BoolLiteralToken(false) },
        { "if", () => new IfToken() },
        { "while", () => new WhileToken() },
        { "for", () => new ForToken() },
        { "in", () => new InToken() }
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
        { '=', () => new EqualsToken() },
        { ';', () => new SemicolonToken() },
        { '+', () => new PlusToken() },
        { '(', () => new LeftParenToken() },
        { ')', () => new RightParenToken() },
        { '{', () => new LeftCurlyToken() },
        { '}', () => new RightCurlyToken() }
    };

    private string? _input;
    private int _currentIndex;

    private char? Peek(int offset = 0)
    {
        var index = _currentIndex + offset;
        if (index >= _input!.Length) return null;

        return _input[index];
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