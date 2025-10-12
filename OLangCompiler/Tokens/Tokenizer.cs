namespace OLangCompiler.Tokens;

public class Tokenizer
{
    private ErrorHelper _errorHelper;
    public List<BaseToken> Tokenize(string input, ErrorHelper errorHelper)
    {
        _errorHelper = errorHelper;
        _input = input;
        _currentIndex = 0;
        _characterNumber = 0;
        _lineNumber = 1;
        var tokens = new List<BaseToken>();
        var startChar = _characterNumber;
        var absoluteCharacterNumber = _absoluteCharacterNumber;
        
        SkipWhitespace();
        var token = GetNextToken();
        SetLineAndCharNumbers(token, startChar, absoluteCharacterNumber);
        
        while (token != null)
        {
            SkipWhitespace();
            tokens.Add(token);
            startChar = _characterNumber;
            absoluteCharacterNumber = _absoluteCharacterNumber;
            token = GetNextToken();
            SetLineAndCharNumbers(token, startChar, absoluteCharacterNumber);
        }

        return tokens;
    }

    private void SkipWhitespace()
    {
        while (Peek() is { } c && char.IsWhiteSpace(c))
        {
            _ = Consume();
        }
    }

    private void SetLineAndCharNumbers(BaseToken? token, int startCharNumber, int absoluteStartCharNumber)
    {
        if (token == null)
        {
            return;
        }
        
        token.RelativeStartCharNumber = startCharNumber;
        token.LineNumber = _lineNumber;
        token.AbsoluteStartCharNumber = absoluteStartCharNumber;

        token.RelativeEndCharNumber = _characterNumber - 1;
        token.AbsoluteEndCharNumber = _absoluteCharacterNumber - 1;
    }

    private BaseToken? GetNextToken()
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
                throw _errorHelper.UnexpectedEndOfInputAfterChar(Peek(-1)!.Value);
            }

            throw _errorHelper.UnexpectedChar(Peek()!.Value);
        }

        if (TryParseOperator() is { } op)
        {
            return op;
        }

        throw new Exception($"Unexpected character: `{Peek()}`");
    }

    private BaseToken? TryParseEqualsToken()
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
    private static readonly Dictionary<char, (Func<BaseToken>, Func<BaseToken>)> EqualsOperatorMap = new()
    {
        { '=', (() => new EqualsToken(), () => new DoubleEqualsToken()) },
        { '+', (() => new PlusToken(), () => new PlusEqualsToken()) },
        { '-', (() => new MinusToken(), () => new MinusEqualsToken()) },
        { '*', (() => new TimesToken(), () => new TimesEqualsToken()) },
        { '!', (() => new NotToken(), () => new NotEqualToken()) },
        { '>', (() => new GreaterToken(), () => new GreaterOrEqualToken()) },
        { '<', (() => new LessToken(), () => new LessOrEqualToken()) }
    };


    private BaseToken TokenizeLetter()
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

    private static readonly Dictionary<string, Func<BaseToken>> KeywordMap = new()
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

    private BaseToken? TryParseOperator()
    {
        if (OperatorMap.ContainsKey(Peek()!.Value))
        {
            return OperatorMap[Consume()].Invoke();
        }

        return null;
    }

    private static readonly Dictionary<char, Func<BaseToken>> OperatorMap = new()
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

    private int _lineNumber;
    private int _characterNumber;
    private int _absoluteCharacterNumber;

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

        _absoluteCharacterNumber++;
        if (ret == '\n')
        {
            _characterNumber = 0;
            _lineNumber++;
        }
        else
        {
            _characterNumber++;
        }

        return (char)ret;
    }
}